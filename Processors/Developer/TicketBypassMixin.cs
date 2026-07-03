#if DO_TICKET_BYPASS
using Model.Persistence;
using Serilog;
using System.Buffers.Binary;
using System.Text.RegularExpressions;

namespace Processors.Processors;

public partial class AuthenticateHandler
{
    private const string SteamAuthWhitelistKey = "STEAM_AUTH_WHITELIST";
    private static readonly Regex SteamId64Pattern = new(@"7656119\d{10}", RegexOptions.Compiled);
    private static readonly Lazy<HashSet<string>> SteamAuthWhitelist = new(LoadSteamAuthWhitelist);

    partial void ApplySteamAuthWhitelistFallback(AuthenticateHandlerRequest request, ref string? steamId64)
    {
        HashSet<string> whitelist = SteamAuthWhitelist.Value;
        if (whitelist.Count == 0)
        {
            return;
        }

        string[] matchingSteamIds = ExtractSteamIdCandidates(request.providerToken)
            .Where(whitelist.Contains)
            .Distinct()
            .Take(2)
            .ToArray();

        if (matchingSteamIds.Length == 1)
        {
            steamId64 = matchingSteamIds[0];
            Log.Warning("Accepted Steam auth ticket using compiled {WhitelistKey}. steamId64={SteamId64}",
                SteamAuthWhitelistKey, steamId64);
            return;
        }

        if (matchingSteamIds.Length > 1)
        {
            Log.Warning("Rejected Steam auth ticket because it matched multiple whitelisted SteamIDs");
        }
    }

    private static HashSet<string> LoadSteamAuthWhitelist()
    {
        string? value = PostgresDatabase.Get().GetConfiguration()[SteamAuthWhitelistKey];
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split([',', ';', ' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static HashSet<string> ExtractSteamIdCandidates(string authTicket)
    {
        HashSet<string> candidates = [];
        Match match = SteamId64Pattern.Match(authTicket);
        while (match.Success)
        {
            if (IsSteamId64(match.Value))
            {
                candidates.Add(match.Value);
            }

            match = match.NextMatch();
        }

        byte[]? bytes = TryDecodeHex(authTicket);
        if (bytes is null)
        {
            return candidates;
        }

        for (int offset = 0; offset + sizeof(ulong) <= bytes.Length; offset++)
        {
            string candidate = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(offset, sizeof(ulong))).ToString();
            if (IsSteamId64(candidate))
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private static byte[]? TryDecodeHex(string value)
    {
        if (value.Length < 16 || (value.Length & 1) != 0)
        {
            return null;
        }

        byte[] bytes = new byte[value.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            int hi = HexNibble(value[i * 2]);
            int lo = HexNibble(value[(i * 2) + 1]);
            if (hi < 0 || lo < 0)
            {
                return null;
            }

            bytes[i] = (byte)((hi << 4) | lo);
        }

        return bytes;
    }

    private static int HexNibble(char c)
    {
        return c is >= '0' and <= '9' ? c - '0' :
            c is >= 'A' and <= 'F' ? c - 'A' + 10 :
            c is >= 'a' and <= 'f' ? c - 'a' + 10 : -1;
    }
}
#endif