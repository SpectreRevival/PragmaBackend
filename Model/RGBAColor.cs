using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public struct RGBAColor : IInterchangeable<RGBAColor, Packets.RGBAColor>
{
    [SetsRequiredMembers]
    public RGBAColor(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    [PgName("r")]
    public required byte R { get; set; }
    [PgName("g")]
    public required byte G { get; set; }
    [PgName("b")]
    public required byte B { get; set; }
    [PgName("a")]
    public required byte A { get; set; }

    public static RGBAColor FromPacket(Packets.RGBAColor inst)
    {
        return new RGBAColor((byte)inst.R, (byte)inst.G, (byte)inst.B, (byte)inst.A);
    }

    public Packets.RGBAColor ToPacket()
    {
        return new Packets.RGBAColor()
        {
            R = R,
            G = G,
            B = B,
            A = A
        };
    }
}