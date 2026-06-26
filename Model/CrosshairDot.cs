using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class CrosshairDot
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
    public required Int32 ColorIndex { get; set; }
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
}