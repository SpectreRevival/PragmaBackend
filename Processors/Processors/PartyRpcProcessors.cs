using Google.Protobuf;
using Model.Persistence;
using Npgsql;
using Packets;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public abstract partial class PartyRpcProcessorBase : WebsocketPacketProcessor
{
    private const string DefaultPartyExtVersion = "173322";
    private const string DefaultLobbyMode = "standard_casual";
    private const string DefaultCrossplayPlatform = "CROSS_PLAY_PLATFORM_PC";
    private const string DefaultPreferredZone = "uscentral-1";
    private const string DefaultPreferredTeam = "TEAM0";
    private const string InviteCodeChars = "ABCDEFGHIJgKLMNOPQRSTUVWXYZ";
    private const int InviteCodeLength = 6;

    protected static readonly ConcurrentDictionary<Guid, SemaphoreSlim> PartyLocks = new();
    protected static readonly ConcurrentDictionary<Guid, PendingPartyInvite> PendingInvites = new();

    private static readonly SpectreRpcType PartyDetailsNotificationType =
        new("PartyRpc.PartyDetailsV1Notification");
    protected static readonly SpectreRpcType InviteReceivedNotificationType =
        new("PartyRpc.InviteReceivedV1Notification");
    protected static readonly SpectreRpcType InviteResponseNotificationType =
        new("PartyRpc.InviteResponseV1Notification");

    private static readonly JsonFormatter ProtoFormatter = new(
        new JsonFormatter.Settings(true)
            .WithFormatDefaultValues(true)
            .WithFormatEnumsAsIntegers(true)
            .WithIndentation("")
            .WithPreserveProtoFieldNames(true));

    private static readonly JsonParser ProtoParser = new(JsonParser.Settings.Default);
    private static readonly JsonParser ProtoParserTolerant = new(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

    protected sealed record PendingPartyInvite(
        Guid InviteId,
        Guid PartyId,
        Guid InviterPlayerId,
        Guid InviteePlayerId,
        string Blob);

    [SetsRequiredMembers]
    protected PartyRpcProcessorBase(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    protected static async Task<Model.Party> GetPartyOrThrow(string partyId)
    {
        Model.Party? party = await Model.Party.RetrieveFromDatabase(Guid.Parse(partyId));
        if (party is null)
        {
            Log.Warning("PARTYDIAG: GetPartyOrThrow MISS for party {PartyId} (client thinks it is in a party the backend does not have)", partyId);
            throw new InvalidDataException($"No party found for id {partyId}");
        }
        return party;
    }

    protected static async Task<SpectreWebsocketMessage> CreatePartySyncMessage(Guid playerId)
    {
        List<Guid> partyIds = await Model.Party.FindPartyIdsContainingPlayer(playerId);
        Model.Party? party = partyIds.Count == 0 ? null : await Model.Party.RetrieveFromDatabase(partyIds[0]);
        Log.Information("PARTYDIAG: party sync/init for {PlayerId}: {Result}", playerId, party is null ? "NOT in party" : $"in party {party.PartyId} v{party.Version} pool={party.QueuePool}");
        if (party is null)
        {
            return SpectreWebsocketMessage.From("{\"isInParty\":false}");
        }

        JsonObject partyRoot = JsonNode.Parse(await BuildPartyJson(party))!.AsObject();
        JsonNode? partyNode = partyRoot["party"];
        partyRoot.Remove("party");
        JsonObject response = new()
        {
            ["isInParty"] = true,
            ["party"] = partyNode
        };
        return SpectreWebsocketMessage.From(response);
    }

    protected static async Task<SpectreWebsocketMessage> CreatePartyMessage(Model.Party party, WebsocketNotification[]? notifs = null)
    {
        return SpectreWebsocketMessage.From(await BuildPartyJson(party), notifs);
    }

    protected static async Task<SpectreWebsocketMessage> CreatePartyMessageWithMemberFanout(Model.Party party, Guid excludePlayerId)
    {
        string partyJson = await BuildPartyJson(party);
        return SpectreWebsocketMessage.From(partyJson, postResponseActions:
        [
            async () => await NotifyOtherMembersBestEffort(party, excludePlayerId, partyJson)
        ]);
    }

    protected static async Task<string> BuildPartyJson(Model.Party party)
    {
        PartyResponse response = await BuildPartyResponse(party);
        return PostProcessPartyJson(ProtoFormatter.Format(response));
    }

    protected static async Task NotifyOtherMembersBestEffort(Model.Party party, Guid excludePlayerId, string partyJson)
    {
        WebsocketNotification notification = new(partyJson, PartyDetailsNotificationType);
        foreach (Model.PartyMember member in party.Members)
        {
            if (member.PlayerId == excludePlayerId)
            {
                continue;
            }

            try
            {
                await SpectreWebsocket.SendNotificationToPlayerAsync(member.PlayerId, notification);
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to send party details notification to {PlayerId}: {Message}", member.PlayerId, ex.Message);
            }
        }
    }

    protected static async Task<T> WithPartyLock<T>(Guid partyId, Func<Task<T>> action)
    {
        SemaphoreSlim partyLock = PartyLocks.GetOrAdd(partyId, _ => new SemaphoreSlim(1, 1));
        await partyLock.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            partyLock.Release();
        }
    }

    protected static async Task<Model.Party> JoinPartyById(Guid partyId, Guid joinerId, JoinRequest joinExt)
    {
        await RemovePlayerFromParties(joinerId, partyId);

        return await WithPartyLock(partyId, async () =>
        {
            Model.Party party = await GetPartyOrThrow(partyId.ToString());
            if (Array.Exists(party.Members, m => m.PlayerId == joinerId))
            {
                return party;
            }

            Model.PartyMember member = new(
                joinerId,
                false,
                false,
                StringOrDefault(joinExt.PreferredTeam, DefaultPreferredTeam),
                await IsRankedModeUnlocked(joinerId),
                ParseVersionOrDefault(joinExt.Version, ParseVersionOrDefault(party.PartyExtVersion, 0)),
                joinExt.Region);

            party.Members = [.. party.Members, member];
            party.Version++;
            await party.SyncToDatabase();
            return party;
        });
    }

    // public so websocket disconnect cleanup can evict dead sessions from their parties
    public static async Task RemovePlayerFromParties(Guid playerId, Guid? exceptPartyId)
    {
        List<Guid> partyIds = await Model.Party.FindPartyIdsContainingPlayer(playerId);
        Log.Information("PARTYDIAG: RemovePlayerFromParties player {PlayerId}, except {Except}, found {Count} party(ies)", playerId, exceptPartyId, partyIds.Count);
        foreach (Guid partyId in partyIds)
        {
            if (exceptPartyId is Guid keep && partyId == keep)
            {
                continue;
            }

            await WithPartyLock(partyId, async () =>
            {
                Model.Party? party = await Model.Party.RetrieveFromDatabase(partyId);
                Model.PartyMember? leaving = party is null ? null : Array.Find(party.Members, m => m.PlayerId == playerId);
                if (party is null || leaving is null)
                {
                    return true;
                }

                Model.PartyMember[] remaining = party.Members.Where(m => m.PlayerId != playerId).ToArray();
                if (remaining.Length == 0)
                {
                    Log.Information("PARTYDIAG: DELETING party {PartyId} (last member {PlayerId} left)", partyId, playerId);
                    await Model.Party.DeleteFromDatabase(partyId);
                    return true;
                }

                if (leaving.IsLeader && !Array.Exists(remaining, m => m.IsLeader))
                {
                    remaining[0] = remaining[0] with { IsLeader = true };
                }

                party.Members = remaining;
                party.Version++;
                await party.SyncToDatabase();
                await NotifyOtherMembersBestEffort(party, playerId, await BuildPartyJson(party));
                return true;
            });
        }
    }

    protected static string? ReadStringField(JsonObject payload, params string[] keys)
    {
        foreach (string key in keys)
        {
            if (payload.TryGetPropertyValue(key, out JsonNode? node)
                && node is JsonValue value
                && value.TryGetValue(out string? parsed)
                && !string.IsNullOrEmpty(parsed))
            {
                return parsed;
            }
        }
        return null;
    }

    protected static bool ReadBoolField(JsonObject payload, bool defaultValue, params string[] keys)
    {
        foreach (string key in keys)
        {
            if (payload.TryGetPropertyValue(key, out JsonNode? node)
                && node is JsonValue value
                && value.TryGetValue(out bool parsed))
            {
                return parsed;
            }
        }
        return defaultValue;
    }

    protected static JoinRequest ReadJoinExt(JsonObject payload)
    {
        foreach (string key in new[] { "playerJoinRequestExt", "playerJoinRequest", "joinRequestExt", "requestExt" })
        {
            if (payload.TryGetPropertyValue(key, out JsonNode? node) && node is JsonObject ext)
            {
                try
                {
                    return ProtoParserTolerant.Parse<JoinRequest>(ext.ToJsonString());
                }
                catch (Exception ex)
                {
                    Log.Warning("Failed to parse party join ext, using defaults: {Message}", ex.Message);
                }
            }
        }
        return new JoinRequest();
    }

    protected static async Task<JsonObject> CreatePlayerInfoJson(Guid playerId)
    {
        Model.ProfileData profile = await Model.ProfileData.RetrieveFromDatabase(playerId)
            ?? throw new InvalidDataException($"No profile found for player {playerId}");
        return new JsonObject
        {
            ["playerId"] = playerId.ToString(),
            ["socialId"] = playerId.ToString(),
            ["displayName"] = new JsonObject
            {
                ["displayName"] = profile.DisplayName.PlayerName,
                ["discriminator"] = profile.DisplayName.Discriminator
            }
        };
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

            string requestedTeam = string.IsNullOrEmpty(update.PreferredTeam) ? member.PreferredTeam : update.PreferredTeam;
            if (requestedTeam != member.PreferredTeam && party.QueuePool == "custom" && IsCustomTeamFull(party, requestedTeam))
            {
                requestedTeam = member.PreferredTeam;
            }

            party.Members[i] = member with
            {
                PreferredTeam = requestedTeam,
                PartyMemberVersion = ParseVersionOrDefault(update.Version, member.PartyMemberVersion),
                Region = update.Region
            };

            if (update.PreferredRegions.Count > 0)
            {
                party.PreferredGameServerZones = update.PreferredRegions.ToArray();
            }

            party.Version++;
            return;
        }
    }

    // custom lobbies are 3v3 + 2 observers; the client UI trusts the backend to block overfill
    private static bool IsCustomTeamFull(Model.Party party, string team)
    {
        int cap = team == "SPECTATOR" ? 2 : 3;
        return party.Members.Count(m => m.PreferredTeam == team) >= cap;
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
            party.Version++;
            return;
        }
    }

    static partial void BeginDeferredMatchmaking(Model.Party party, SpectreWebsocket connection, ref bool deferred);

    protected static async Task<WebsocketNotification[]> QueueMatchmakingNotifications(Model.Party party, SpectreWebsocket connection)
    {
        bool deferred = false;
        BeginDeferredMatchmaking(party, connection, ref deferred);
        if (deferred)
        {
            WebsocketNotification enteredMatchmaking = new(
                "{}",
                new SpectreRpcType("GameInstanceRpc.GameInstanceEnteredMatchmakingV1Notification"));
            foreach (Model.PartyMember member in party.Members)
            {
                if (member.PlayerId == connection.PlayerId)
                {
                    continue;
                }
                await SpectreWebsocket.SendNotificationToPlayerAsync(member.PlayerId, enteredMatchmaking);
            }
            return [enteredMatchmaking];
        }

        GameConnectionDetails details = CreateGameConnectionDetails();
        WebsocketNotification[] leaderNotifications = CreateMatchmakingNotifications(details, true);
        WebsocketNotification[] memberNotifications = CreateMatchmakingNotifications(details, false);

        foreach (Model.PartyMember member in party.Members)
        {
            if (member.PlayerId == connection.PlayerId)
            {
                continue;
            }

            foreach (WebsocketNotification notification in memberNotifications)
            {
                await SpectreWebsocket.SendNotificationToPlayerAsync(member.PlayerId, notification);
            }
        }

        return leaderNotifications;
    }

    private static WebsocketNotification[] CreateMatchmakingNotifications(GameConnectionDetails details,
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
            new WebsocketNotification(
                "{}",
                new SpectreRpcType("GameInstanceRpc.GameInstanceEnteredMatchmakingV1Notification")),
            new WebsocketNotification(
                addedToGame,
                                new SpectreRpcType("GameInstanceRpc.AddedToGameV1Notification")
),
            new WebsocketNotification(
                hostDetails,
                                new SpectreRpcType("GameInstanceRpc.HostConnectionDetailsV1Notification")
)
        ];
    }

    static partial void ApplyGameServerOverrides(GameConnectionDetails details);

    protected static GameConnectionDetails CreateGameConnectionDetails()
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
        ApplyGameServerOverrides(details);
        return details;
    }

    public static int PreferredTeamToTeamValue(string preferredTeam) => preferredTeam switch
    {
        "TEAM0" => 0,
        "TEAM1" => 1,
        "SPECTATOR" => -1,
        _ => 0
    };

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
        string chatChannel = $"party-{modelParty.PartyId:N}";
        broadcast.ChatId = chatChannel;
        if (VivoxTokenGenerator.IsConfigured)
        {
            foreach (Model.PartyMember chatMember in modelParty.Members)
            {
                broadcast.Tokens.Add(new LobbyChatToken
                {
                    PlayerId = chatMember.PlayerId.ToString(),
                    ChatToken = VivoxTokenGenerator.GenerateToken(chatMember.PlayerId, VivoxTokenAction.JOIN, chatChannel)
                });
            }
        }
        broadcast.Version = modelParty.PartyExtVersion;
        broadcast.Region = modelParty.Region;
        broadcast.Tag = modelParty.Tag;
        broadcast.Profile = modelParty.Profile;
        broadcast.UseTeamMmr = modelParty.UseTeamMMR;
        broadcast.HasAcceptableRegion = modelParty.HasAcceptableRegion;
        broadcast.CrossPlayPreference = new CrossplayPreferences { Platform = modelParty.CrossplayPlatform };

        if (!string.IsNullOrEmpty(modelParty.CustomJson))
        {
            CustomGameInfo custom = ProtoParser.Parse<CustomGameInfo>(modelParty.CustomJson);
            custom.TeamAssignments.Clear();
            foreach (Model.PartyMember member in modelParty.Members)
            {
                custom.TeamAssignments.Add(new TeamAssignment
                {
                    PlayerId = member.PlayerId.ToString(),
                    Team = PreferredTeamToTeamValue(member.PreferredTeam)
                });
            }
            broadcast.Custom = custom;
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
        // one query per member; the previous per-item lookups made every lobby update crawl
        // once real parties with full inventories showed up
        foreach (Model.CustomizedInstancedItem item in await Model.CustomizedInstancedItem.RetrieveAllForPlayer(playerId))
        {
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