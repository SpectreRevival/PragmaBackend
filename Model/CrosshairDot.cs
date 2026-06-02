namespace Model;

public record class CrosshairDot
{
    public required double Thickness { get; set; }
    public required double Opacity { get; set; }
    public required Int32 ColorIndex { get; set; }
    public required RGBAColor CustomColor { get; set; }
    public required bool OutlineEnabled { get; set; }
    public required RGBAColor CustomOutlineColor { get; set; }
    public required double OutlineOpacity { get; set; }
    public required double OutlineThickness { get; set; }
}