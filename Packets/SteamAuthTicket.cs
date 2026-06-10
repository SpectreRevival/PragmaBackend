using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace Packets;

public class SteamAuthTicket
{
    private static readonly int GC_TOKEN_LENGTH_FIELD_SIZE = 4;
    private static readonly int GC_TOKEN_SIZE = 20;
    private static readonly int STEAM_ID_OFFSET = 12;
    private static readonly int EXPECTED_GC_TOKEN_SIZE = 20;

    public required string SteamId64 { get; set; }

    [SetsRequiredMembers]
    public SteamAuthTicket(string authTicket)
    {
        int headerBytes = GC_TOKEN_LENGTH_FIELD_SIZE + GC_TOKEN_SIZE;
        int requiredHexChars = headerBytes * 2;
        if(authTicket.Length < requiredHexChars)
        {
            throw new InvalidDataException("Auth ticket does not appear to be valid");
        }
        byte[] bytes = new byte[headerBytes];
        for (int i = 0; i < headerBytes; i++)
        {
            int hi = HexNibble(authTicket[i * 2]);
            int lo = HexNibble(authTicket[(i * 2) + 1]);

            if (hi < 0 || lo < 0)
            {
                throw new InvalidDataException("Failed to decode steam provider id");
            }

            bytes[i] = (byte)((hi << 4) | lo);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes) != EXPECTED_GC_TOKEN_SIZE)
        {
            throw new InvalidDataException("GC token size was not what was expected");
        }

        ulong steamId = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(STEAM_ID_OFFSET));

        if (((steamId >> 56) & 0xFFU) != 1)
        {
            throw new InvalidDataException("Steam ID was not valid");
        }
        if(((steamId >> 52) & 0xFU) != 1)
        {
            throw new InvalidDataException("Steam ID was not valid");
        }
        SteamId64 = steamId.ToString();

    }

    private static int HexNibble(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'A' && c <= 'F') return c - 'A' + 10;
        if (c >= 'a' && c <= 'f') return c - 'a' + 10;
        return -1;
    }
}