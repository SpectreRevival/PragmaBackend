using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Model;
using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Packets.Processors;

public class AuthenticateHandler : HTTPPacketHandler, IHTTPPacketHandlerSingleton
{
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

    private static Guid PlayerIdFromSteamId(string steamId)
    {
        // Creates a new GUID using steamId as the seed so the same steamId will always yield the same playerId;
        var rand = new Random(steamId.GetHashCode());
        byte[] guidBytes = new byte[16];
        rand.NextBytes(guidBytes);
        return new Guid(guidBytes);
    }

    private static async Task<Model.ProfileData> CreateNewPlayerFromSteamId(string steamId)
    {
        Guid playerId = PlayerIdFromSteamId(steamId);

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
        await attackerOutfitLoadout.SyncToDatabase();
        Model.OutfitLoadout defenderOutfitLoadout = Model.OutfitLoadout.CreateDefault(playerId);
        await defenderOutfitLoadout.SyncToDatabase();
        Model.WeaponLoadout attackerWeaponLoadout = Model.WeaponLoadout.CreateDefault(playerId);
        await attackerWeaponLoadout.SyncToDatabase();
        Model.WeaponLoadout defenderWeaponLoadout = Model.WeaponLoadout.CreateDefault(playerId);
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
        // TODO instantiate the display name from the steam ID given to resolve persona name using the steam API
        await playerProfile.SyncToDatabase();
        return playerProfile;
    }

    public record AuthenticateHandlerRequest(string providerId, string providerToken, string gameShardId, string loginQueuePassToken);

    public record PragmaTokenPair(string pragmaGameToken, string pragmaSocialToken);

    public record AuthenticateHandlerResponse(PragmaTokenPair pragmaTokens)
    {
    }
}