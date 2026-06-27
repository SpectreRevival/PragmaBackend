using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Npgsql;
using Persistence;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Packets.Processors;

public class AuthenticateHandler : HTTPPacketHandler, IHTTPPacketHandlerSingleton
{
    // short timeout so a slow/down steam web api can't hang the auth request; we fall back to the stored name.
    private static readonly HttpClient SteamHttpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

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

        if(reqData == null)
        {
            return Results.BadRequest();
        }

        NpgsqlCommand cmd = PostgresDatabase.Get().GetRaw().CreateCommand(
            "SELECT player_id FROM profile_data WHERE provider_account_id = @provider_account_id");
        SteamAuthTicket ticket;
        try
        {
            ticket = new(reqData.providerToken);
        } catch(Exception ex)
        {
            return Results.BadRequest(ex);
        }
        cmd.Parameters.AddWithValue("provider_account_id", ticket.SteamId64);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        Model.ProfileData playerProfile;
        if (!await reader.ReadAsync())
        {
            playerProfile = await CreateNewPlayerFromSteamId(ticket.SteamId64);
        } else
        {
            Guid playerId = await reader.GetFieldValueAsync<Guid>(0);
            await reader.DisposeAsync();
            playerProfile = await Model.ProfileData.RetrieveFromDatabase(playerId);
        }

        // the jwt carries displayName, so the steam persona name has to be resolved and persisted before we build it.
        string? steamApiKey = Request.RequestServices.GetRequiredService<IConfiguration>()["STEAM_WEB_API_KEY"];
        if (!string.IsNullOrWhiteSpace(steamApiKey))
        {
            string? personaName = await ResolveSteamPersonaName(ticket.SteamId64, steamApiKey);
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

        return Results.Json(new AuthenticateHandlerResponse(new PragmaTokenPair(
            BuildJWT("GAME", playerProfile),
            BuildJWT("SOCIAL", playerProfile)
            )));
    }

    private static string BuildJWT(string backendType, Model.ProfileData profile)
    {
        var jwtHeader = new JsonObject
        {
            ["kid"] = "d3JtOq6jy3_HquwTsrzt81wh3BLiA-4f-qM8mj-0-YQ=",
            ["alg"] = "RS256",
            ["typ"] = "JWT"
        };
        var payload = new JsonObject
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

        if(backendType == "GAME")
        {
            payload["gameShardId"] = "00000000-0000-0000-0000-000000000001";
        }

        var headerString = jwtHeader.ToJsonString();
        var payloadString = payload.ToJsonString();
        string encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(headerString));
        string encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadString));
        string stringToSign = $"{encodedHeader}.{encodedPayload}";
        byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
        RSA pragmaKey = RSA.Create(2048); // Just create a new 2048 bit key every time, the client actually doesn't care who the signature is from so long as its signed
        byte[] signature = pragmaKey.SignData(bytesToSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
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

    private static Guid PlayerIdFromSteamId(string steamId)
    {
        // Creates a new GUID using steamId as the seed so the same steamId will always yield the same playerId;
        byte[] steamIdBytes = Encoding.UTF8.GetBytes(steamId);
        byte[] hashBytes = SHA256.HashData(steamIdBytes);
        return new Guid(hashBytes.AsSpan(0, 16));
    }

    private static void FixupOutfitData(Model.OutfitData data, Guid playerId)
    {
        NpgsqlCommand cmd = PostgresDatabase.CreateCommand("SELECT instance_id FROM instanced_items WHERE owning_player_id=@player_id AND catalog_id=@catalog_id");
        cmd.Parameters.AddWithValue("player_id", playerId);
        cmd.Parameters.AddWithValue("catalog_id", data.ItemCatalogId);
        using var reader = cmd.ExecuteReader();
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
        NpgsqlCommand cmd = PostgresDatabase.CreateCommand("SELECT instance_id FROM instanced_items WHERE owning_player_id=@player_id AND catalog_id=@catalog_id");
        cmd.Parameters.AddWithValue("player_id", playerId);
        cmd.Parameters.AddWithValue("catalog_id", data.ItemCatalogId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidDataException($"The player with id {playerId} doesn't own an item with catalog id {data.ItemCatalogId}");
        }
        data.ItemInstanceId = reader.GetGuid(0);
        if (data.Attachment != null)
        {
            NpgsqlCommand attachmentCmd = PostgresDatabase.CreateCommand("SELECT instance_id FROM instanced_items WHERE owning_player_id=@player_id AND catalog_id=@catalog_id");
            attachmentCmd.Parameters.AddWithValue("player_id", playerId);
            attachmentCmd.Parameters.AddWithValue("catalog_id", data.Attachment.AttachmentItemCatalogId);
            using var attachmentReader = attachmentCmd.ExecuteReader();
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
        NpgsqlCommand cmd = PostgresDatabase.CreateCommand("SELECT instance_id FROM instanced_items WHERE catalog_id=@catalog_id AND owning_player_id=@player_id");
        cmd.Parameters.AddWithValue("catalog_id", catalogId);
        cmd.Parameters.AddWithValue("player_id", owningPlayerId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidDataException($"No item found with catalog id {catalogId} and owning player id {owningPlayerId}");
        }
        return reader.GetGuid(0);
    }

    private static async Task<Model.ProfileData> CreateNewPlayerFromSteamId(string steamId)
    {
        Guid playerId = PlayerIdFromSteamId(steamId);
        DefaultInventory defaultInv = JsonSerializer.Deserialize<DefaultInventory>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "Inventory.json")), new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
        foreach (var stackableItem in defaultInv.StackableItems)
        {
            stackableItem.InstanceId = Guid.NewGuid();
            stackableItem.OwningPlayerId = playerId;
            await stackableItem.SyncToDatabase();
        }
        foreach (var customizedInstancedItem in defaultInv.CustomizedInstancedItems)
        {
            customizedInstancedItem.InstanceId = Guid.NewGuid();
            customizedInstancedItem.OwningPlayerId = playerId;
            await customizedInstancedItem.SyncToDatabase();
        }
        foreach (var progressionTrackerItem in defaultInv.ProgresionTrackingItems)
        {
            progressionTrackerItem.InstanceId = Guid.NewGuid();
            progressionTrackerItem.OwningPlayerId = playerId;
            await progressionTrackerItem.SyncToDatabase();
        }
        foreach (var sponsorTrackerItem in defaultInv.SponsorUnlockItems)
        {
            sponsorTrackerItem.InstanceId = Guid.NewGuid();
            sponsorTrackerItem.OwningPlayerId = playerId;
            await sponsorTrackerItem.SyncToDatabase();
        }
        Model.BattlepassData bpData = Model.BattlepassData.CreateDefault(playerId);
        await bpData.SyncToDatabase();
        Model.ColorVisionConfig colorVisionConfig = Model.ColorVisionConfig.CreateDefault(playerId);
        await colorVisionConfig.SyncToDatabase();
        Model.CrosshairConfig crosshairCfg = Model.CrosshairConfig.CreateDefault(playerId);
        await crosshairCfg.SyncToDatabase();
        IndividualTrackedProgression individualProg = IndividualTrackedProgression.CreateDefault(playerId);
        await individualProg.SyncToDatabase();
        TeamTrackedProgression teamProg = TeamTrackedProgression.CreateDefault(playerId);
        await teamProg.SyncToDatabase();
        Model.PlayerMatchmakingData mmData = Model.PlayerMatchmakingData.CreateDefault(playerId);
        await mmData.SyncToDatabase();
        Model.LegacySeasonData lgSeason = Model.LegacySeasonData.CreateDefault(playerId);
        await lgSeason.SyncToDatabase();
        for (LegacyStatsType type = 0; type < LegacyStatsType.Team + 1; type++)
        {
            Model.LegacyStatsData statsData = Model.LegacyStatsData.CreateDefault(new LegacyStatsDataKey(playerId, type));
            await statsData.SyncToDatabase();
        }
        Model.FriendsList friends = Model.FriendsList.CreateDefault(playerId);
        await friends.SyncToDatabase();
        Model.GamepadConfig gamepadCfg = Model.GamepadConfig.CreateDefault(playerId);
        await gamepadCfg.SyncToDatabase();
        Model.OutfitLoadout attackerOutfitLoadout = Model.OutfitLoadout.CreateDefault(playerId);
        FixupOutfitLoadout(attackerOutfitLoadout, playerId);
        await attackerOutfitLoadout.SyncToDatabase();
        Model.OutfitLoadout defenderOutfitLoadout = Model.OutfitLoadout.CreateDefault(playerId);
        FixupOutfitLoadout(defenderOutfitLoadout, playerId);
        await defenderOutfitLoadout.SyncToDatabase();
        Model.WeaponLoadout attackerWeaponLoadout = Model.WeaponLoadout.CreateDefault(playerId);
        FixupWeaponLoadout(attackerWeaponLoadout, playerId);
        await attackerWeaponLoadout.SyncToDatabase();
        Model.WeaponLoadout defenderWeaponLoadout = Model.WeaponLoadout.CreateDefault(playerId);
        FixupWeaponLoadout(defenderWeaponLoadout, playerId);
        await defenderWeaponLoadout.SyncToDatabase();
        Model.SubtitleUserSettings subtitleSettings = Model.SubtitleUserSettings.CreateDefault(playerId);
        await subtitleSettings.SyncToDatabase();
        Model.PlayerPresence presence = Model.PlayerPresence.CreateDefault(playerId);
        await presence.SyncToDatabase();
        Model.PlayerConfig playerConfig = Model.PlayerConfig.CreateDefault(playerId);
        await playerConfig.SyncToDatabase();
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
        await playerProfile.SyncToDatabase();
        return playerProfile;
    }

    public record AuthenticateHandlerRequest(string providerId, string providerToken, string gameShardId, string loginQueuePassToken);

    public record PragmaTokenPair(string pragmaGameToken, string pragmaSocialToken);

    public record AuthenticateHandlerResponse(PragmaTokenPair pragmaTokens)
    {
    }
}