using Google.Protobuf;
using Model.Persistence;
using Npgsql;
using Packets;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public abstract class PartyRpcProcessorBase : WebsocketPacketProcessor
{
    private const string DefaultPartyExtVersion = "173322";
    private const string DefaultLobbyMode = "standard_casual";
    private const string DefaultCrossplayPlatform = "CROSS_PLAY_PLATFORM_PC";
    private const string DefaultPreferredZone = "uscentral-1";
    private const string InviteCodeChars = "ABCDEFGHIJgKLMNOPQRSTUVWXYZ";
    private const int InviteCodeLength = 6;

    private static readonly JsonFormatter ProtoFormatter = new(
        new JsonFormatter.Settings(true)
            .WithFormatDefaultValues(true)
            .WithFormatEnumsAsIntegers(true)
            .WithIndentation("")
            .WithPreserveProtoFieldNames(true));

    private static readonly JsonParser ProtoParser = new(JsonParser.Settings.Default);

    [SetsRequiredMembers]
    protected PartyRpcProcessorBase(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    protected static async Task<Model.Party> GetPartyOrThrow(string partyId)
    {
        Model.Party? party = await Model.Party.RetrieveFromDatabase(Guid.Parse(partyId));
        return party is null ? throw new InvalidDataException($"No party found for id {partyId}") : party;
    }

    protected static async Task<SpectreWebsocketMessage> CreatePartyMessage(Model.Party party)
    {
        PartyResponse response = await BuildPartyResponse(party);
        return SpectreWebsocketMessage.From(PostProcessPartyJson(ProtoFormatter.Format(response)));
    }

    protected static async Task<Model.Party> CreateParty(CreatePartyRequest request, Guid playerId)
    {
        CreateRequest createRequest = request.CreateRequestExt ?? new CreateRequest();
        JoinRequest joinRequest = request.PlayerJoinRequestExt ?? new JoinRequest();
        string extVersion = StringOrDefault(createRequest.Version, DefaultPartyExtVersion);
        Dictionary<string, string> standard = createRequest.Standard.Count > 0
            ? createRequest.Standard.ToDictionary(entry => entry.Key, entry => entry.Value)
            : new Dictionary<string, string> { ["mode"] = "Standard" };

        Model.PartyMember member = new(
            playerId,
            false,
            true,
            "TEAM0",
            await IsRankedModeUnlocked(playerId),
            ParseVersionOrDefault(joinRequest.Version, ParseVersionOrDefault(extVersion, 0)),
            joinRequest.Region);

        Model.Party party = new(
            Guid.NewGuid(),
            [member],
            await CreateInviteCode(),
            "",
            StringOrDefault(createRequest.LobbyMode, DefaultLobbyMode),
            "",
            createRequest.UseTeamMmr,
            1,
            extVersion,
            createRequest.Region,
            createRequest.Tag,
            createRequest.Profile,
            true,
            request.PreferredGameServerZones.Count > 0
                ? request.PreferredGameServerZones.ToArray()
                : [DefaultPreferredZone],
            standard,
            "",
            StringOrDefault(createRequest.CrossPlayPreference?.Platform, DefaultCrossplayPlatform));

        await party.SyncToDatabase();
        return party;
    }

    protected static void ApplyPartyUpdate(Model.Party party, PartyUpdate update)
    {
        string pool = update.Pool;
        if (string.IsNullOrEmpty(pool) && !string.IsNullOrEmpty(update.LobbyMode))
        {
            pool = update.LobbyMode;
        }

        party.QueuePool = pool;
        party.LobbyMode = update.LobbyMode;
        party.PartyExtVersion = StringOrDefault(update.Version, party.PartyExtVersion);
        party.Region = update.Region;
        party.Tag = update.Tag;
        party.Profile = update.Profile;
        party.UseTeamMMR = update.UseTeamMmr;
        party.HasAcceptableRegion = update.AcceptableRegions.Count > 0 || party.HasAcceptableRegion;

        if (pool == "custom")
        {
            party.Standard = [];
            party.CustomJson = update.Custom is null ? "" : ProtoFormatter.Format(update.Custom);
        }
        else
        {
            party.Standard = update.Standard.Count > 0
                ? update.Standard.ToDictionary(entry => entry.Key, entry => entry.Value)
                : new Dictionary<string, string> { ["mode"] = "Standard" };
            party.CustomJson = "";
        }

        party.Version++;
    }

    protected static void ApplyPartyPlayerUpdate(Model.Party party, Guid playerId, PartyPlayerUpdateData update)
    {
        for (int i = 0; i < party.Members.Length; i++)
        {
            Model.PartyMember member = party.Members[i];
            if (member.PlayerId != playerId)
            {
                continue;
            }

            party.Members[i] = member with
            {
                PreferredTeam =
                string.IsNullOrEmpty(update.PreferredTeam) ? member.PreferredTeam : update.PreferredTeam,
                PartyMemberVersion = ParseVersionOrDefault(update.Version, member.PartyMemberVersion),
                Region = update.Region
            };

            if (update.PreferredRegions.Count > 0)
            {
                party.PreferredGameServerZones = update.PreferredRegions.ToArray();
            }

            return;
        }
    }

    protected static void ApplyReadyState(Model.Party party, Guid playerId, bool ready)
    {
        for (int i = 0; i < party.Members.Length; i++)
        {
            Model.PartyMember member = party.Members[i];
            if (member.PlayerId != playerId)
            {
                continue;
            }

            party.Members[i] = member with { IsReady = ready };
            return;
        }
    }

    protected static async Task QueueMatchmakingNotifications(Model.Party party, SpectreWebsocket connection)
    {
        GameConnectionDetails details = CreateGameConnectionDetails();
        SpectreWebsocketNotification[] leaderNotifications = CreateMatchmakingNotifications(details, true);
        SpectreWebsocketNotification[] memberNotifications = CreateMatchmakingNotifications(details, false);

        foreach (Model.PartyMember member in party.Members)
        {
            if (member.PlayerId == connection.PlayerId)
            {
                continue;
            }

            foreach (SpectreWebsocketNotification notification in memberNotifications)
            {
                await SpectreWebsocket.SendNotificationToPlayerAsync(member.PlayerId, notification.RpcType,
                    notification.Payload);
            }
        }

        foreach (SpectreWebsocketNotification notification in leaderNotifications)
        {
            connection.QueuePostResponseNotification(notification.RpcType, notification.Payload);
        }
    }

    private static SpectreWebsocketNotification[] CreateMatchmakingNotifications(GameConnectionDetails details,
        bool matchLeader)
    {
        AddedToGameV1Notification addedToGame = new()
        {
            GameInstanceId = details.GameInstanceId,
            Ext = new AddedToGameExt { MatchLeader = matchLeader }
        };

        HostConnectionDetailsV1Notification hostDetails = new()
        {
            ConnectionDetails = details
        };

        return
        [
            new SpectreWebsocketNotification(
                new SpectreRpcType("GameInstanceRpc.GameInstanceEnteredMatchmakingV1Notification"),
                SpectreWebsocketMessage.From("{}")),
            new SpectreWebsocketNotification(
                new SpectreRpcType("GameInstanceRpc.AddedToGameV1Notification"),
                SpectreWebsocketMessage.From(addedToGame)),
            new SpectreWebsocketNotification(
                new SpectreRpcType("GameInstanceRpc.HostConnectionDetailsV1Notification"),
                SpectreWebsocketMessage.From(hostDetails))
        ];
    }

    private static GameConnectionDetails CreateGameConnectionDetails()
    {
        GameConnectionDetails details = new()
        {
            GameInstanceId = GetEnvString("SPECTRE_GAME_INSTANCE_ID", "185206f8-dae2-4144-923d-9ac326240f30"),
            Hostname = GetEnvString("SPECTRE_GAME_HOST", "127.0.0.1"),
            Port = GetEnvInt("SPECTRE_GAME_PORT", 7777),
            ConnectionToken = GetEnvString("SPECTRE_GAME_CONNECTION_TOKEN", "8656593e-1d5c-4a94-b460-190f7b694c95"),
            ExtConnectionDetails = new ExtConnectionDetails
            {
                MatchData = GetEnvString("SPECTRE_GAME_MATCH_DATA", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")
            }
        };
        return details;
    }

    private static async Task<PartyResponse> BuildPartyResponse(Model.Party modelParty)
    {
        PartyResponse response = new()
        {
            Party = new Party
            {
                ExtBroadcastParty = new BroadcastPartyExtraInfo()
            }
        };
        Party packetParty = response.Party;
        packetParty.PartyId = modelParty.PartyId.ToString();
        packetParty.InviteCode = modelParty.InviteCode;
        packetParty.Version = modelParty.Version.ToString();
        packetParty.PreferredGameServerZones.Add(modelParty.PreferredGameServerZones);

        BroadcastPartyExtraInfo broadcast = packetParty.ExtBroadcastParty;
        broadcast.Pool = modelParty.QueuePool;
        broadcast.LobbyMode = modelParty.LobbyMode;
        broadcast.ChatId = modelParty.ChatId;
        broadcast.Version = modelParty.PartyExtVersion;
        broadcast.Region = modelParty.Region;
        broadcast.Tag = modelParty.Tag;
        broadcast.Profile = modelParty.Profile;
        broadcast.UseTeamMmr = modelParty.UseTeamMMR;
        broadcast.HasAcceptableRegion = modelParty.HasAcceptableRegion;
        broadcast.CrossPlayPreference = new CrossplayPreferences { Platform = modelParty.CrossplayPlatform };

        if (!string.IsNullOrEmpty(modelParty.CustomJson))
        {
            broadcast.Custom = ProtoParser.Parse<CustomGameInfo>(modelParty.CustomJson);
        }
        else
        {
            broadcast.Standard.Add(modelParty.Standard);
        }

        foreach (Model.PartyMember member in modelParty.Members)
        {
            packetParty.PartyMembers.Add(await BuildPartyMember(member));
        }

        return response;
    }

    private static async Task<PartyMember> BuildPartyMember(Model.PartyMember modelMember)
    {
        Model.ProfileData profile = await Model.ProfileData.RetrieveFromDatabase(modelMember.PlayerId)
                                    ?? throw new InvalidDataException(
                                        $"No profile found for party member {modelMember.PlayerId}");

        PartyMember member = new()
        {
            Ext = new PartyMemberExtraInfo(),
            PlayerId = modelMember.PlayerId.ToString(),
            SocialId = modelMember.PlayerId.ToString(),
            IsReady = modelMember.IsReady,
            IsLeader = modelMember.IsLeader,
            DisplayName = new DisplayName
            {
                DisplayName_ = profile.DisplayName.PlayerName,
                Discriminator = profile.DisplayName.Discriminator
            }
        };

        PartyMemberExtraInfo ext = member.Ext;
        ext.Version = modelMember.PartyMemberVersion <= 0 ? "" : modelMember.PartyMemberVersion.ToString();
        ext.Region = modelMember.Region;
        ext.PreferredTeam = modelMember.PreferredTeam;
        ext.RankedModeUnlocked = modelMember.RankedModeUnlocked;
        ext.SharedClientData = CreateSharedClientData(profile);
        ext.PlayerData = await CreatePartyMemberPlayerData(profile);
        ext.LimitedInstancedInventory.Add(await CreateLimitedInstancedInventory(modelMember.PlayerId));

        return member;
    }

    private static PartySharedClientData CreateSharedClientData(Model.ProfileData profile)
    {
        PartySharedClientData sharedData = new()
        {
            AccountIdProvider = profile.AccountIdProvider,
            PlatformName = profile.PlatformName,
            CrossPlayPlatformKind = profile.CrossplayPlatformKind,
            CurrentProviderAccountId = profile.ProviderAccountId,
            CrewId = profile.CrewId == Guid.Empty ? "" : profile.CrewId.ToString(),
            GamesRemainingUntilCrewJoin = (uint)Math.Max(0, profile.GamesRemainingUntilCrewJoin),
            BattlePassLevel = 0
        };
        return sharedData;
    }

    private static async Task<PartyMemberPlayerData> CreatePartyMemberPlayerData(Model.ProfileData profile)
    {
        PartyMemberPlayerData data = new()
        {
            AttackerWeaponLoadout = new WeaponLoadoutReference
            {
                PlayerId = profile.PlayerId.ToString(),
                LoadoutId = profile.AttackerWeaponLoadoutId.ToString()
            },
            DefenderWeaponLoadout = new WeaponLoadoutReference
            {
                PlayerId = profile.PlayerId.ToString(),
                LoadoutId = profile.DefenderWeaponLoadoutId.ToString()
            },

            AttackerOutfitLoadout = (await Model.OutfitLoadout.RetrieveFromDatabase(profile.AttackerOutfitLoadoutId)).ToPacket(),
            DefenderOutfitLoadout = (await Model.OutfitLoadout.RetrieveFromDatabase(profile.DefenderOutfitLoadoutId)).ToPacket(),
            MatchmakingData = (await Model.PlayerMatchmakingData.RetrieveFromDatabase(profile.PlayerId)).ToPacket(),
            Banner = await CreateFlatInstancedItem(profile.BannerItemId)
        };
        return data;
    }

    private static async Task<FlatInstancedItem> CreateFlatInstancedItem(Guid instanceId)
    {
        Model.CustomizedInstancedItem? model = await Model.CustomizedInstancedItem.RetrieveFromDatabase(instanceId);
        FlatInstancedItem packet = new()
        {
            ItemInstanceId = instanceId.ToString()
        };
        if (model is null)
        {
            return packet;
        }

        packet.ItemCatalogId = model.CatalogId;
        foreach (Model.AlterationChannel channel in model.AlterationChannels)
        {
            foreach (string alterationId in channel.Alterations)
            {
                packet.AlterationData.Add(new ActiveAlterationData
                {
                    ChannelId = channel.ChannelId,
                    AlterationId = alterationId
                });
            }
        }

        return packet;
    }

    private static async Task<InstancedItem[]> CreateLimitedInstancedInventory(Guid playerId)
    {
        List<InstancedItem> items = [];
        await using NpgsqlCommand cmd = PostgresDatabase.CreateCommand(
            "SELECT instance_id FROM customized_instanced_items WHERE owning_player_id = @player_id AND alteration_channels IS NOT NULL");
        cmd.Parameters.AddWithValue("player_id", playerId);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        List<Guid> itemIds = [];
        while (await reader.ReadAsync())
        {
            itemIds.Add(reader.GetGuid(0));
        }

        foreach (Guid itemId in itemIds)
        {
            Model.CustomizedInstancedItem? item = await Model.CustomizedInstancedItem.RetrieveFromDatabase(itemId);
            if (item is null)
            {
                continue;
            }

            InstancedItem packet = new()
            {
                CatalogId = item.CatalogId,
                InstanceId = item.InstanceId.ToString(),
                Ext = new InstanceExtData
                {
                    Reserved = 0,
                    Viewed = item.Viewed,
                    InstancedCustomizationData = new InstancedCustomizationData()
                }
            };

            foreach (Model.AlterationChannel channel in item.AlterationChannels)
            {
                AlterationChannel packetChannel = new()
                {
                    ChannelId = channel.ChannelId
                };
                packetChannel.OwnedAlterations.Add(channel.Alterations);
                packet.Ext.InstancedCustomizationData.InstancedAlterationChannels.Add(packetChannel);
            }

            items.Add(packet);
        }

        return items.ToArray();
    }

    private static async Task<bool> IsRankedModeUnlocked(Guid playerId)
    {
        Model.PlayerConfig? playerConfig = await Model.PlayerConfig.RetrieveFromDatabase(playerId);
        if (playerConfig?.UnlockAllPlayModes == true)
        {
            return true;
        }

        Model.PlayerMatchmakingData? matchmakingData = await Model.PlayerMatchmakingData.RetrieveFromDatabase(playerId);
        return matchmakingData?.CasualMatchesWon >= 15;
    }

    private static async Task<string> CreateInviteCode()
    {
        while (true)
        {
            string inviteCode = CreateRandomInviteCode();
            await using NpgsqlCommand cmd =
                PostgresDatabase.CreateCommand(
                    "SELECT 1 FROM parties WHERE upper(invite_code) = upper(@invite_code) LIMIT 1");
            cmd.Parameters.AddWithValue("invite_code", inviteCode);
            object? existing = await cmd.ExecuteScalarAsync();
            if (existing is null)
            {
                return inviteCode;
            }
        }
    }

    private static string CreateRandomInviteCode()
    {
        char[] code = new char[InviteCodeLength];
        for (int i = 0; i < code.Length; i++)
        {
            code[i] = InviteCodeChars[RandomNumberGenerator.GetInt32(InviteCodeChars.Length)];
        }

        return new string(code);
    }

    private static string StringOrDefault(string? value, string defaultValue)
    {
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    private static long ParseVersionOrDefault(string? value, long defaultValue)
    {
        return long.TryParse(value, out long parsed) ? parsed : defaultValue;
    }

    private static string GetEnvString(string name, string defaultValue)
    {
        string? value = PostgresDatabase.Get().GetConfiguration()[name];
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    private static int GetEnvInt(string name, int defaultValue)
    {
        string? value = PostgresDatabase.Get().GetConfiguration()[name];
        return int.TryParse(value, out int parsed) ? parsed : defaultValue;
    }

    private static string PostProcessPartyJson(string json)
    {
        JsonObject root = JsonNode.Parse(json)!.AsObject();
        JsonObject? party = root["party"]?.AsObject();
        JsonObject? broadcast = party?["extBroadcastParty"]?.AsObject();

        RemoveEmptyObject(broadcast, "standard");
        RemoveEmptyObject(broadcast, "custom");

        if (party?["partyMembers"] is JsonArray members)
        {
            foreach (JsonNode? member in members)
            {
                JsonObject? ext = member?["ext"]?.AsObject();
                if (ext?["sharedClientData"] is JsonObject sharedClientData)
                {
                    ext["sharedClientData"] =
                        sharedClientData.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
                }
            }
        }

        return root.ToJsonString();
    }

    private static void RemoveEmptyObject(JsonObject? parent, string propertyName)
    {
        if (parent?[propertyName] is JsonObject obj && obj.Count == 0)
        {
            parent.Remove(propertyName);
        }
    }
}

public class CreatePartyProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public CreatePartyProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.CreateV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        Model.Party party =
            await CreateParty(Packet.GetPayloadAsMessage<CreatePartyRequest>(), ConnectionHandler.PlayerId);
        return await CreatePartyMessage(party);
    }
}

public class UpdatePartyProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public UpdatePartyProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.UpdatePartyV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        UpdatePartyRequest request = Packet.GetPayloadAsMessage<UpdatePartyRequest>();
        Model.Party party = await GetPartyOrThrow(request.PartyId);
        ApplyPartyUpdate(party, request.RequestExt ?? new PartyUpdate());
        await party.SyncToDatabase();
        return await CreatePartyMessage(party);
    }
}

public class UpdatePartyPlayerProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public UpdatePartyPlayerProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.UpdatePartyPlayerV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        UpdatePartyPlayerRequest request = Packet.GetPayloadAsMessage<UpdatePartyPlayerRequest>();
        Model.Party party = await GetPartyOrThrow(request.PartyId);
        ApplyPartyPlayerUpdate(party, ConnectionHandler.PlayerId, request.RequestExt ?? new PartyPlayerUpdateData());
        await party.SyncToDatabase();
        return await CreatePartyMessage(party);
    }
}

public class SetReadyProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public SetReadyProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.SetReadyStateV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        SetReadyMessage request = Packet.GetPayloadAsMessage<SetReadyMessage>();
        Model.Party party = await GetPartyOrThrow(request.PartyId);
        ApplyReadyState(party, ConnectionHandler.PlayerId, request.Ready);
        await party.SyncToDatabase();
        return await CreatePartyMessage(party);
    }
}

public class EnterMatchmakingProcessor : PartyRpcProcessorBase, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public EnterMatchmakingProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PartyRpc.EnterMatchmakingV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        EnterMatchmakingRequest request = Packet.GetPayloadAsMessage<EnterMatchmakingRequest>();
        Model.Party party = await GetPartyOrThrow(request.PartyId);
        await QueueMatchmakingNotifications(party, ConnectionHandler);
        return await CreatePartyMessage(party);
    }
}