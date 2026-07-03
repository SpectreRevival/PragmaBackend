#if DO_TICKET_BYPASS
using Serilog;
using System.Buffers.Binary;
using System.Text.RegularExpressions;

namespace Processors.Processors;

public partial class AuthenticateHandler
{
    private const ulong SteamId64Base = 76561197960265728UL;
    private static readonly Regex SteamIdPattern = new(@"7656119\d{10}", RegexOptions.Compiled);

    partial void ApplyTicketBypass(AuthenticateHandlerRequest request, TicketBypassContext context)
    {
        string token = request.providerToken ?? string.Empty;

        string? steamId = ExtractSteamId64(token, out int candidateCount);
        if (steamId == null)
        {
            Log.Warning("DO_TICKET_BYPASS rejected a Steam auth ticket because candidate count was {CandidateCount}",
                candidateCount);
            return;
        }

        context.Enabled = true;
        context.SteamId64 = steamId;

        Log.Warning(
            "DO_TICKET_BYPASS accepted a Steam auth ticket using token SteamID extraction. steamId64={SteamId64} candidateCount={CandidateCount}",
            steamId, candidateCount);
    }

    private static string? ExtractSteamId64(string token, out int candidateCount)
    {
        List<string> candidates = [];

        Match match = SteamIdPattern.Match(token);
        while (match.Success)
        {
            if (IsValidSteamId64(match.Value))
            {
                candidates.Add(match.Value);
            }

            match = match.NextMatch();
        }

        candidates.AddRange(ExtractSteamIdsFromHex(token));

        string[] uniqueCandidates = candidates.Distinct().ToArray();
        candidateCount = uniqueCandidates.Length;
        return uniqueCandidates.Length == 1 ? uniqueCandidates[0] : null;
    }

    private static List<string> ExtractSteamIdsFromHex(string token)
    {
        if (token.Length < 16 || (token.Length & 1) != 0)
        {
            return [];
        }

        byte[] bytes = new byte[token.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            int hi = HexNibble(token[i * 2]);
            int lo = HexNibble(token[(i * 2) + 1]);
            if (hi < 0 || lo < 0)
            {
                return [];
            }

            bytes[i] = (byte)((hi << 4) | lo);
        }

        List<string> candidates = [];
        for (int offset = 0; offset + sizeof(ulong) <= bytes.Length; offset++)
        {
            string candidate = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(offset, sizeof(ulong))).ToString();
            if (IsValidSteamId64(candidate))
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private static bool IsValidSteamId64(string? value)
    {
        return ulong.TryParse(value, out ulong steamId)
               && steamId > SteamId64Base
               && steamId <= SteamId64Base + uint.MaxValue;
    }

    private static int HexNibble(char c)
    {
        return c is >= '0' and <= '9' ? c - '0' :
            c is >= 'A' and <= 'F' ? c - 'A' + 10 :
            c is >= 'a' and <= 'f' ? c - 'a' + 10 : -1;
    }
}
#endif