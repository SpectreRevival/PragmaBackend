using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class CrosshairDot : IInterchangeable<CrosshairDot, Packets.CrosshairDot>
{
    [SetsRequiredMembers]
    public CrosshairDot(double thickness, double opacity, int colorIndex, RGBAColor customColor, bool outlineEnabled, RGBAColor customOutlineColor, double outlineOpacity, double outlineThickness)
    {
        Thickness = thickness;
        Opacity = opacity;
        ColorIndex = colorIndex;
        CustomColor = customColor;
        OutlineEnabled = outlineEnabled;
        CustomOutlineColor = customOutlineColor;
        OutlineOpacity = outlineOpacity;
        OutlineThickness = outlineThickness;
    }

    public CrosshairDot()
    {

    }

    [PgName("thickness")]
    public required double Thickness { get; set; }
    [PgName("opacity")]
    public required double Opacity { get; set; }
    [PgName("color_index")]
    public required int ColorIndex { get; set; }
    [PgName("custom_color")]
    public required RGBAColor CustomColor { get; set; }
    [PgName("outline_enabled")]
    public required bool OutlineEnabled { get; set; }
    [PgName("custom_outline_color")]
    public required RGBAColor CustomOutlineColor { get; set; }
    [PgName("outline_opacity")]
    public required double OutlineOpacity { get; set; }
    [PgName("outline_thickness")]
    public required double OutlineThickness { get; set; }

    public static CrosshairDot FromPacket(Packets.CrosshairDot inst)
    {
        return new CrosshairDot(inst.Thickness, inst.Opacity, inst.ColorIndex, RGBAColor.FromPacket(inst.CustomColor), inst.BOutlineEnabled, RGBAColor.FromPacket(inst.CustomOutlineColor), inst.OutlineOpacity, inst.OutlineThickness);
    }

    public Packets.CrosshairDot ToPacket()
    {
        Packets.CrosshairDot packet = new()
        {
            Thickness = Thickness,
            Opacity = Opacity,
            ColorIndex = ColorIndex
        };
        Packets.RGBAColor customColor = new()
        {
            R = CustomColor.R,
            G = CustomColor.G,
            B = CustomColor.B,
            A = CustomColor.A
        };
        packet.CustomColor = customColor;
        packet.BOutlineEnabled = OutlineEnabled;
        Packets.RGBAColor outlineColor = new()
        {
            R = CustomOutlineColor.R,
            G = CustomOutlineColor.G,
            B = CustomOutlineColor.B,
            A = CustomOutlineColor.A
        };
        packet.CustomOutlineColor = outlineColor;
        packet.OutlineOpacity = OutlineOpacity;
        packet.OutlineThickness = OutlineThickness;
        return packet;
    }
}