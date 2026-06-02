namespace Model;

public record class ColorVisionConfig : VersionedData
{
    public required string ColorVisionType { get; set; }
    public required Int32 Severity { get; set; }
    public required bool CorrectDeficiency { get; set; }
    public required bool ShowCorrectDeficiency { get; set; }
    public required bool ComfortSwapEffect { get; set; }
    public required bool CustomOutlineColor { get; set; }
    public required RGBAColor OutlineColor { get; set; }
    public required RGBAColor OutlineColorLower { get; set; }
    public required double OutlineThicknessScale { get; set; }
    public required double OutlineBrightnessScale { get; set; }
}