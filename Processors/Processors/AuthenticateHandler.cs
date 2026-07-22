using Microsoft.AspNetCore.Http;
using Model;
using Model.Persistence;
using Npgsql;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public partial class AuthenticateHandler : HTTPPacketHandler, IHTTPPacketHandlerSingleton
{
    private const string SteamAppId = "2641470";

    // short timeout so a slow/down steam web api can't hang the auth request; we fall back to the stored name.
    private static readonly HttpClient SteamHttpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static readonly RSA PragmaSigningKey = RSA.Create(2048);
    private static readonly object PragmaSigningKeyLock = new();

    [SetsRequiredMembers]
    public AuthenticateHandler(HttpMethod method, string route) : base(method, route)
    {
    }

    public static HttpMethod GetMethod()
    {
        return HttpMethod.Post;
    }

    public static string GetRoute()
    {
        return "/v1/account/authenticateorcreatev2";
    }

    public override async Task<IResult> HandleAsync(HttpContext Request)
    {
        AuthenticateHandlerRequest? reqData = await JsonSerializer.DeserializeAsync<AuthenticateHandlerRequest>(
          Request.Request.Body,
          JsonSerializerOptions.Web
        );

        if (reqData == null)
        {
            return Results.BadRequest();
        }
        if (string.IsNullOrWhiteSpace(reqData.providerId))
        {
            return Results.BadRequest();
        }

        string providerId = reqData.providerId.ToUpperInvariant();
        if (providerId != "STEAM")
        {
            return Results.BadRequest($"Unsupported providerId {reqData.providerId}");
        }

        string steamId64;
        string authSource;
        Microsoft.Extensions.Configuration.IConfiguration config = PostgresDatabase.Get().GetConfiguration();
        string? steamApiKey = config["STEAM_WEB_API_KEY"];
        string? whitelistedSteamId = null;
        ApplySteamAuthWhitelistFallback(reqData, ref whitelistedSteamId);
        SteamTicketAuthenticationResult authResult = new(SteamTicketAuthenticationStatus.Unavailable);

        if (!string.IsNullOrWhiteSpace(steamApiKey))
        {
            authResult = await AuthenticateSteamUserTicket(reqData.providerToken, steamApiKey, SteamAppId);
            if (authResult.Status == SteamTicketAuthenticationStatus.Success && !string.IsNullOrWhiteSpace(authResult.SteamId64))
            {
                steamId64 = authResult.SteamId64;
                authSource = "SteamWebApi";
                Log.Information("Steam AuthenticateUserTicket accepted ticket. steamId64={SteamId64}", steamId64);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(whitelistedSteamId))
                {
                    return authResult.Status == SteamTicketAuthenticationStatus.Unavailable
                        ? Results.BadRequest("Steam ticket authentication was unavailable")
                        : Results.BadRequest("Invalid Steam auth ticket");
                }

                steamId64 = whitelistedSteamId;
                authSource = "SteamAuthWhitelist";
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(whitelistedSteamId))
            {
                Log.Warning("STEAM_WEB_API_KEY not configured and auth ticket did not match compiled Steam auth whitelist");
                return Results.BadRequest("Steam Web API key is required unless ticket matches Steam auth whitelist");
            }

            steamId64 = whitelistedSteamId;
            authSource = "SteamAuthWhitelist";
        }

        NpgsqlCommand cmd = PostgresDatabase.Get().GetRaw().CreateCommand(
            "SELECT player_id FROM profile_data WHERE account_id_provider = @account_id_provider AND provider_account_id = @provider_account_id");
        cmd.Parameters.AddWithValue("account_id_provider", providerId);
        cmd.Parameters.AddWithValue("provider_account_id", steamId64);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        Model.ProfileData playerProfile;
        if (!await reader.ReadAsync())
        {
            playerProfile = await CreateNewPlayerFromSteamId(steamId64);
        }
        else
        {
            Guid playerId = await reader.GetFieldValueAsync<Guid>(0);
            await reader.DisposeAsync();
            playerProfile = await Model.ProfileData.RetrieveFromDatabase(playerId);
        }

        // the jwt carries displayName, so the steam persona name has to be resolved and persisted before we build it.
        if (!string.IsNullOrWhiteSpace(steamApiKey))
        {
            string? personaName = await ResolveSteamPersonaName(steamId64, steamApiKey);
            if (!string.IsNullOrEmpty(personaName) && personaName != playerProfile.DisplayName.PlayerName)
            {
                playerProfile.DisplayName.PlayerName = personaName;
                await playerProfile.SyncToDatabase();
            }
        }
        else
        {
            Log.Warning("STEAM_WEB_API_KEY not configured; using stored display name");
        }

        Log.Information(
            "Authenticated Steam player. playerId={PlayerId} steamId64={SteamId64} displayName={DisplayName} authSource={AuthSource}",
            playerProfile.PlayerId,
            steamId64,
            playerProfile.DisplayName.PlayerName,
            authSource);

        return Results.Json(new AuthenticateHandlerResponse(new PragmaTokenPair(
            BuildJWT("GAME", playerProfile),
            BuildJWT("SOCIAL", playerProfile)
            )));
    }

    private enum SteamTicketAuthenticationStatus
    {
        Success,
        Rejected,
        Unavailable
    }

    private sealed record SteamTicketAuthenticationResult(SteamTicketAuthenticationStatus Status, string? SteamId64 = null);

    partial void ApplySteamAuthWhitelistFallback(AuthenticateHandlerRequest request, ref string? steamId64);

    private static string BuildJWT(string backendType, Model.ProfileData profile)
    {
        JsonObject jwtHeader = new()
        {
            ["kid"] = "d3JtOq6jy3_HquwTsrzt81wh3BLiA-4f-qM8mj-0-YQ=",
            ["alg"] = "RS256",
            ["typ"] = "JWT"
        };
        JsonObject payload = new()
        {
            ["iss"] = "pragma",
            ["sub"] = profile.PlayerId.ToString(),
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["exp"] = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
            ["jti"] = Guid.NewGuid().ToString(),
            ["sessionType"] = "PLAYER",
            ["backendType"] = backendType,
            ["displayName"] = profile.DisplayName.PlayerName,
            ["discriminator"] = profile.DisplayName.Discriminator,
            ["pragmaSocialId"] = profile.PlayerId.ToString(),
            ["idProvider"] = "STEAM",
            ["extSessionInfo"] = "{\"permissions\":0,\"accountTags\":[\"canary\"]}",
            ["expiresInMillis"] = "86400000",
            ["refreshInMillis"] = "36203000",
            ["pragmaPlayerId"] = profile.PlayerId.ToString()
        };

        if (backendType == "GAME")
        {
            payload["gameShardId"] = "00000000-0000-0000-0000-000000000001";
        }

        string headerString = jwtHeader.ToJsonString();
        string payloadString = payload.ToJsonString();
        string encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(headerString));
        string encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadString));
        string stringToSign = $"{encodedHeader}.{encodedPayload}";
        byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
        byte[] signature;
        lock (PragmaSigningKeyLock)
        {
            signature = PragmaSigningKey.SignData(bytesToSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        string encodedSignature = Base64UrlEncode(signature);
        return $"{stringToSign}.{encodedSignature}";
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static async Task<SteamTicketAuthenticationResult> AuthenticateSteamUserTicket(string authTicket, string apiKey, string appId)
    {
        try
        {
            string url = $"https://api.steampowered.com/ISteamUserAuth/AuthenticateUserTicket/v1/?key={apiKey}&appid={Uri.EscapeDataString(appId)}&ticket={Uri.EscapeDataString(authTicket)}";
            using HttpResponseMessage resp = await SteamHttpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                Log.Warning("Steam AuthenticateUserTicket returned {StatusCode}", (int)resp.StatusCode);
                return new SteamTicketAuthenticationResult(SteamTicketAuthenticationStatus.Unavailable);
            }

            using Stream stream = await resp.Content.ReadAsStreamAsync();
            using JsonDocument doc = await JsonDocument.ParseAsync(stream);
            if (!doc.RootElement.TryGetProperty("response", out JsonElement response))
            {
                Log.Warning("Steam AuthenticateUserTicket response did not contain a response object");
                return new SteamTicketAuthenticationResult(SteamTicketAuthenticationStatus.Unavailable);
            }

            if (response.TryGetProperty("params", out JsonElement parameters))
            {
                string? result = parameters.TryGetProperty("result", out JsonElement resultElement)
                    ? resultElement.GetString()
                    : null;
                string? steamId = parameters.TryGetProperty("steamid", out JsonElement steamIdElement)
                    ? steamIdElement.GetString()
                    : null;

                if (string.Equals(result, "OK", StringComparison.OrdinalIgnoreCase) && IsSteamId64(steamId))
                {
                    return new SteamTicketAuthenticationResult(SteamTicketAuthenticationStatus.Success, steamId);
                }

                Log.Warning("Steam AuthenticateUserTicket rejected ticket with result {Result}", result ?? "<missing>");
                return new SteamTicketAuthenticationResult(SteamTicketAuthenticationStatus.Rejected);
            }

            if (response.TryGetProperty("error", out JsonElement error))
            {
                string? errorDescription = error.TryGetProperty("errordesc", out JsonElement errorDescElement)
                    ? errorDescElement.GetString()
                    : null;
                Log.Warning("Steam AuthenticateUserTicket rejected ticket: {ErrorDescription}", errorDescription ?? "<missing>");
                return new SteamTicketAuthenticationResult(SteamTicketAuthenticationStatus.Rejected);
            }

            Log.Warning("Steam AuthenticateUserTicket response did not contain params or error");
            return new SteamTicketAuthenticationResult(SteamTicketAuthenticationStatus.Unavailable);
        }
        catch (Exception ex)
        {
            Log.Warning("Failed to authenticate Steam ticket: {Message}", ex.Message);
            return new SteamTicketAuthenticationResult(SteamTicketAuthenticationStatus.Unavailable);
        }
    }

    private static async Task<string?> ResolveSteamPersonaName(string steamId64, string apiKey)
    {
        try
        {
            string url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={apiKey}&steamids={steamId64}";
            using HttpResponseMessage resp = await SteamHttpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                Log.Warning($"Steam GetPlayerSummaries returned {(int)resp.StatusCode} for steamId {steamId64}");
                return null;
            }
            using Stream stream = await resp.Content.ReadAsStreamAsync();
            using JsonDocument doc = await JsonDocument.ParseAsync(stream);
            JsonElement players = doc.RootElement.GetProperty("response").GetProperty("players");
            if (players.GetArrayLength() == 0)
            {
                Log.Warning($"Steam returned no summary for steamId {steamId64}");
                return null;
            }
            return players[0].GetProperty("personaname").GetString();
        }
        catch (Exception ex)
        {
            Log.Warning($"Failed to resolve steam persona name for {steamId64}: {ex.Message}");
            return null;
        }
    }

    private static bool IsSteamId64(string? value)
    {
        const ulong steamId64Base = 76561197960265728UL;
        return ulong.TryParse(value, out ulong steamId)
            && steamId > steamId64Base
            && steamId <= steamId64Base + uint.MaxValue;
    }

    private static Guid PlayerIdFromSteamId(string steamId)
    {
        // Creates a new GUID using steamId as the seed so the same steamId will always yield the same playerId;
        byte[] steamIdBytes = Encoding.UTF8.GetBytes(steamId);
        byte[] hashBytes = SHA256.HashData(steamIdBytes);
        return new Guid(hashBytes.AsSpan(0, 16));
    }

    private static void FixupOutfitData(Model.OutfitData data, Guid playerId)
    {
        NpgsqlCommand cmd = PostgresDatabase.CreateCommand("SELECT instance_id FROM customized_instanced_items WHERE owning_player_id=@player_id AND catalog_id=@catalog_id");
        cmd.Parameters.AddWithValue("player_id", playerId);
        cmd.Parameters.AddWithValue("catalog_id", data.ItemCatalogId);
        using NpgsqlDataReader reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidDataException($"The player with id {playerId} doesn't own an item with catalog id {data.ItemCatalogId}");
        }
        data.ItemInstanceId = reader.GetGuid(0);
    }

    private static void FixupOutfitLoadout(Model.OutfitLoadout loadout, Guid playerId)
    {
        FixupOutfitData(loadout.Outfit, playerId);
        FixupOutfitData(loadout.Hair, playerId);
        FixupOutfitData(loadout.FaceStyle, playerId);
        FixupOutfitData(loadout.FaceAccessory, playerId);
        FixupOutfitData(loadout.Head, playerId);
    }

    private static void FixupWeaponData(Model.WeaponData data, Guid playerId)
    {
        NpgsqlCommand cmd = PostgresDatabase.CreateCommand("SELECT instance_id FROM customized_instanced_items WHERE owning_player_id=@player_id AND catalog_id=@catalog_id");
        cmd.Parameters.AddWithValue("player_id", playerId);
        cmd.Parameters.AddWithValue("catalog_id", data.ItemCatalogId);
        using NpgsqlDataReader reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidDataException($"The player with id {playerId} doesn't own an item with catalog id {data.ItemCatalogId}");
        }
        data.ItemInstanceId = reader.GetGuid(0);
        if (data.Attachment != null)
        {
            NpgsqlCommand attachmentCmd = PostgresDatabase.CreateCommand("SELECT instance_id FROM customized_instanced_items WHERE owning_player_id=@player_id AND catalog_id=@catalog_id");
            attachmentCmd.Parameters.AddWithValue("player_id", playerId);
            attachmentCmd.Parameters.AddWithValue("catalog_id", data.Attachment.AttachmentItemCatalogId);
            using NpgsqlDataReader attachmentReader = attachmentCmd.ExecuteReader();
            if (!attachmentReader.Read())
            {
                throw new InvalidDataException($"The player with id {playerId} doesn't own an item with catalog id {data.Attachment.AttachmentItemCatalogId}");
            }
            data.Attachment.AttachmentItemInstanceId = reader.GetGuid(0);
        }
    }

    private static void FixupWeaponLoadout(Model.WeaponLoadout loadout, Guid playerId)
    {
        FixupWeaponData(loadout.SemiAutoPistol, playerId);
        FixupWeaponData(loadout.SuppressedPistol, playerId);
        FixupWeaponData(loadout.AutoPistol, playerId);
        FixupWeaponData(loadout.HighcalPistol, playerId);
        FixupWeaponData(loadout.HeavyShotgun, playerId);
        FixupWeaponData(loadout.AutoShotgun, playerId);
        FixupWeaponData(loadout.TacticalSMG, playerId);
        FixupWeaponData(loadout.RapidfireSMG, playerId);
        FixupWeaponData(loadout.SuppressedSMG, playerId);
        FixupWeaponData(loadout.StandardAR, playerId);
        FixupWeaponData(loadout.SemiAutoAR, playerId);
        FixupWeaponData(loadout.BurstAR, playerId);
        FixupWeaponData(loadout.TacticalAR, playerId);
        FixupWeaponData(loadout.SuppressedAR, playerId);
        FixupWeaponData(loadout.HeavyAR, playerId);
        FixupWeaponData(loadout.HighcalMG, playerId);
        FixupWeaponData(loadout.RapidfireMG, playerId);
        FixupWeaponData(loadout.SemiAutoSniper, playerId);
        FixupWeaponData(loadout.BoltActionSniper, playerId);
        FixupWeaponData(loadout.Melee, playerId);
    }

    private static Guid GetInstanceIdByCatalogId(string catalogId, Guid owningPlayerId)
    {
        NpgsqlCommand cmd = PostgresDatabase.CreateCommand("SELECT instance_id FROM customized_instanced_items WHERE catalog_id=@catalog_id AND owning_player_id=@player_id");
        cmd.Parameters.AddWithValue("catalog_id", catalogId);
        cmd.Parameters.AddWithValue("player_id", owningPlayerId);
        using NpgsqlDataReader reader = cmd.ExecuteReader();
        return !reader.Read()
            ? throw new InvalidDataException($"No item found with catalog id {catalogId} and owning player id {owningPlayerId}")
            : reader.GetGuid(0);
    }

    private static async Task<Model.ProfileData> CreateNewPlayerFromSteamId(string steamId)
    {
        NpgsqlBatch batch = new NpgsqlBatch(PostgresDatabase.Get().GetRaw().CreateConnection());
        Guid playerId = PlayerIdFromSteamId(steamId);
        using var conn = PostgresDatabase.Get().GetRaw().CreateConnection();
        using var stackableBulkWriter = Model.StackableItem.CreateBulkWriter();
        foreach (Model.StackableItem stackableItem in DefaultInventory.Get().StackableItems)
        {
            stackableItem.InstanceId = Guid.NewGuid();
            stackableItem.OwningPlayerId = playerId;
            await stackableItem.WriteToBulkWriter(stackableBulkWriter);
        }
        using var instancedItemWriter = Model.CustomizedInstancedItem.CreateBulkWriter();
        foreach (CustomizedInstancedItem customizedInstancedItem in DefaultInventory.Get().CustomizedInstancedItems)
        {
            customizedInstancedItem.InstanceId = Guid.NewGuid();
            customizedInstancedItem.OwningPlayerId = playerId;
            await customizedInstancedItem.WriteToBulkWriter(instancedItemWriter);
        }
        using var progressionItemWriter = Model.ProgressionTrackingItem.CreateBulkWriter();
        foreach (ProgressionTrackingItem progressionTrackerItem in DefaultInventory.Get().ProgresionTrackingItems)
        {
            progressionTrackerItem.InstanceId = Guid.NewGuid();
            progressionTrackerItem.OwningPlayerId = playerId;
            await progressionTrackerItem.WriteToBulkWriter(progressionItemWriter);
        }
        using var sponsorUnlockItemWriter = Model.SponsorUnlockTrackerItem.CreateBulkWriter();
        foreach (SponsorUnlockTrackerItem sponsorTrackerItem in DefaultInventory.Get().SponsorUnlockItems)
        {
            sponsorTrackerItem.InstanceId = Guid.NewGuid();
            sponsorTrackerItem.OwningPlayerId = playerId;
            await sponsorTrackerItem.WriteToBulkWriter(sponsorUnlockItemWriter);
        }
        Model.BattlepassData bpData = Model.BattlepassData.CreateDefault(playerId);
        batch.BatchCommands.Add(bpData.CreateBatchSyncCommand());
        Model.ColorVisionConfig colorVisionConfig = Model.ColorVisionConfig.CreateDefault(playerId);
        batch.BatchCommands.Add(colorVisionConfig.CreateBatchSyncCommand());
        Model.CrosshairConfig crosshairCfg = Model.CrosshairConfig.CreateDefault(playerId);
        batch.BatchCommands.Add(crosshairCfg.CreateBatchSyncCommand());
        IndividualTrackedProgression individualProg = IndividualTrackedProgression.CreateDefault(playerId);
        batch.BatchCommands.Add(individualProg.CreateBatchSyncCommand());
        TeamTrackedProgression teamProg = TeamTrackedProgression.CreateDefault(playerId);
        batch.BatchCommands.Add(teamProg.CreateBatchSyncCommand());
        Model.PlayerMatchmakingData mmData = Model.PlayerMatchmakingData.CreateDefault(playerId);
        batch.BatchCommands.Add(mmData.CreateBatchSyncCommand());
        Model.LegacySeasonData lgSeason = Model.LegacySeasonData.CreateDefault(playerId);
        batch.BatchCommands.Add(lgSeason.CreateBatchSyncCommand());
        for (LegacyStatsType type = 0; type < LegacyStatsType.Team + 1; type++)
        {
            Model.LegacyStatsData statsData = Model.LegacyStatsData.CreateDefault(new LegacyStatsDataKey(playerId, type));
            batch.BatchCommands.Add(statsData.CreateBatchSyncCommand());
        }
        Model.FriendsList friends = Model.FriendsList.CreateDefault(playerId);
        batch.BatchCommands.Add(friends.CreateBatchSyncCommand());
        Model.GamepadConfig gamepadCfg = Model.GamepadConfig.CreateDefault(playerId);
        batch.BatchCommands.Add(gamepadCfg.CreateBatchSyncCommand());
        Model.OutfitLoadout attackerOutfitLoadout = Model.OutfitLoadout.CreateDefault(playerId);
        FixupOutfitLoadout(attackerOutfitLoadout, playerId);
        batch.BatchCommands.Add(attackerOutfitLoadout.CreateBatchSyncCommand());
        Model.OutfitLoadout defenderOutfitLoadout = Model.OutfitLoadout.CreateDefault(playerId);
        FixupOutfitLoadout(defenderOutfitLoadout, playerId);
        batch.BatchCommands.Add(defenderOutfitLoadout.CreateBatchSyncCommand());
        Model.WeaponLoadout attackerWeaponLoadout = Model.WeaponLoadout.CreateDefault(playerId);
        FixupWeaponLoadout(attackerWeaponLoadout, playerId);
        batch.BatchCommands.Add(attackerWeaponLoadout.CreateBatchSyncCommand());
        Model.WeaponLoadout defenderWeaponLoadout = Model.WeaponLoadout.CreateDefault(playerId);
        FixupWeaponLoadout(defenderWeaponLoadout, playerId);
        batch.BatchCommands.Add(defenderWeaponLoadout.CreateBatchSyncCommand());
        Model.SubtitleUserSettings subtitleSettings = Model.SubtitleUserSettings.CreateDefault(playerId);
        batch.BatchCommands.Add(subtitleSettings.CreateBatchSyncCommand());
        Model.PlayerPresence presence = Model.PlayerPresence.CreateDefault(playerId);
        batch.BatchCommands.Add(presence.CreateBatchSyncCommand());
        Model.PlayerConfig playerConfig = Model.PlayerConfig.CreateDefault(playerId);
        batch.BatchCommands.Add(playerConfig.CreateBatchSyncCommand());
        Model.ProfileData playerProfile = Model.ProfileData.CreateDefault(playerId);
        playerProfile.DefenderOutfitLoadoutId = defenderOutfitLoadout.LoadoutId;
        playerProfile.AttackerOutfitLoadoutId = attackerOutfitLoadout.LoadoutId;
        playerProfile.DefenderWeaponLoadoutId = defenderWeaponLoadout.LoadoutId;
        playerProfile.AttackerWeaponLoadoutId = attackerWeaponLoadout.LoadoutId;
        playerProfile.LastLogin = DateTimeOffset.UtcNow;
        playerProfile.LastUpdated = DateTimeOffset.UtcNow;
        playerProfile.ProviderAccountId = steamId;
        playerProfile.PreSprayItemId = GetInstanceIdByCatalogId("SpectreSprayItemDef:SprayID_Default_01", playerId);
        playerProfile.MatchSprayItemId = GetInstanceIdByCatalogId("SpectreSprayItemDef:SprayID_Default_01", playerId);
        playerProfile.PostSprayItemId = GetInstanceIdByCatalogId("SpectreSprayItemDef:SprayID_Default_01", playerId);
        playerProfile.BannerItemId = GetInstanceIdByCatalogId("SpectreBannerItemDef:BannerID_Track_Kit01_District_01", playerId);
        batch.BatchCommands.Add(playerProfile.CreateBatchSyncCommand());
        ClientMessage cyberlordKnifeMessage = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "staticdata", "CyberlordMessage.json"))).Deserialize<ClientMessage>(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new UnixDateTimeOffsetConverter() }
        });
        cyberlordKnifeMessage.PlayerId = playerId;
        cyberlordKnifeMessage.MessageId = Guid.NewGuid();
        batch.BatchCommands.Add(cyberlordKnifeMessage.CreateBatchSyncCommand());
        await batch.ExecuteNonQueryAsync();
        return playerProfile;
    }

    public record AuthenticateHandlerRequest(string providerId, string providerToken, string gameShardId, string loginQueuePassToken);

    public record PragmaTokenPair(string pragmaGameToken, string pragmaSocialToken);

    public record AuthenticateHandlerResponse(PragmaTokenPair pragmaTokens)
    {
    }
}