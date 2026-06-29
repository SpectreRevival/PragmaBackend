using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class PipConfig : IInterchangeable<PipConfig, Packets.PipConfig>
{
    [SetsRequiredMembers]
    public PipConfig(double thickness, double length, double opacity, double offset, bool moveAccuracyOffset, bool fireAccuracyOffset)
    {
        Thickness = thickness;
        Length = length;
        Opacity = opacity;
        Offset = offset;
        MoveAccuracyOffset = moveAccuracyOffset;
        FireAccuracyOffset = fireAccuracyOffset;
    }

    public PipConfig()
    {

    }

    [PgName("thickness")]
    public required double Thickness { get; set; }
    [PgName("piplen")]
    public required double Length { get; set; }
    [PgName("opacity")]
    public required double Opacity { get; set; }
    [PgName("pip_offset")]
    public required double Offset { get; set; }
    [PgName("move_accuracy_offset")]
    public required bool MoveAccuracyOffset { get; set; }
    [PgName("fire_accuracy_offset")]
    public required bool FireAccuracyOffset { get; set; }

    public static PipConfig FromPacket(Packets.PipConfig inst)
    {
        return new PipConfig(inst.Thickness, inst.Length, inst.Opacity, inst.Offset, inst.BMoveAccuracyOffset, inst.BFireAccuracyOffset);
    }

    public Packets.PipConfig ToPacket()
    {
        Packets.PipConfig packet = new()
        {
            Thickness = Thickness,
            Length = Length,
            Opacity = Opacity,
            Offset = Offset,
            BMoveAccuracyOffset = MoveAccuracyOffset,
            BFireAccuracyOffset = FireAccuracyOffset
        };
        return packet;
    }
}