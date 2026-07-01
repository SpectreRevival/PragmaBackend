using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Packets;

public class VivoxTokenGenerator
{
    private static string? secretKey;
    public static string? issuer;
    public static string? domain;
    private static int _globalVxi = 0;
    public static string? server;

    public static string GenerateToken(Guid playerId, VivoxTokenAction action, string channel)
    {
        long exp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 100;
        int vxi = Interlocked.Increment(ref _globalVxi);

        string from = $"sip:.{issuer}.{playerId}.@{domain}";

        Dictionary<string, object> payloadObj = new()
        {
            { "iss", issuer },
            { "exp", exp },
            { "vxa", action },
            { "vxi", vxi },
            { "f", from }
        };

        if (action == VivoxTokenAction.JOIN)
        {
            payloadObj["t"] = $"sip:confctl-g-{issuer}.{channel}@{domain}";
        }

        string payloadJson = JsonSerializer.Serialize(payloadObj);

        string header = "e30";
        string payloadB64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payloadJson));
        string signingInput = $"{header}.{payloadB64}";

        byte[] secretBytes = Encoding.UTF8.GetBytes(secretKey);
        byte[] inputBytes = Encoding.UTF8.GetBytes(signingInput);

        using HMACSHA256 hmac = new(secretBytes);
        byte[] hashBytes = hmac.ComputeHash(inputBytes);
        string signature = Base64UrlEncoder.Encode(hashBytes);

        return $"{signingInput}.{signature}";
    }

    public static bool LoadConfiguration(IConfiguration config)
    {
        secretKey = config.GetSection("Vivox").GetSection("SecretKey").ToString();
        issuer = config.GetSection("Vivox").GetSection("Issuer").ToString();
        domain = config.GetSection("Vivox").GetSection("Domain").ToString();
        server = config.GetSection("Vivox").GetSection("Server").ToString();
        return secretKey is not null && issuer is not null && domain is not null && server is not null;
    }
}

public enum VivoxTokenAction
{
    LOGIN,
    JOIN
}