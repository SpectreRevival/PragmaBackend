using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Packets;

public class VivoxTokenGenerator
{
    public static string? secretKey;
    public static string? issuer;
    public static string? domain;
    public static string? server;

    private static long _globalVxi = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public static bool IsConfigured =>
        !string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(issuer)
        && !string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(server);

    public static string GenerateToken(Guid playerId, VivoxTokenAction action, string channel)
    {

        long exp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 90;
        long vxi = Interlocked.Increment(ref _globalVxi);

        string from = $"sip:.{issuer}.{playerId}.@{domain}";

        Dictionary<string, object> payloadObj = new()
        {
            { "iss", issuer! },
            { "exp", exp },

            { "vxa", action == VivoxTokenAction.JOIN ? "join" : "login" },
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

        byte[] secretBytes = Encoding.UTF8.GetBytes(secretKey!);
        byte[] inputBytes = Encoding.UTF8.GetBytes(signingInput);

        using HMACSHA256 hmac = new(secretBytes);
        byte[] hashBytes = hmac.ComputeHash(inputBytes);
        string signature = Base64UrlEncoder.Encode(hashBytes);

        return $"{signingInput}.{signature}";
    }

    public static bool LoadConfiguration(IConfiguration config)
    {

        secretKey = config["Vivox:SecretKey"];
        issuer = config["Vivox:Issuer"];
        domain = config["Vivox:Domain"];
        server = config["Vivox:Server"];
        return IsConfigured;
    }
}

public enum VivoxTokenAction
{
    LOGIN,
    JOIN
}