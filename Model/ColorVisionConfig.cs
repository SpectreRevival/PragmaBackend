namespace Model;

public record class ColorVisionConfig : VersionedData, IDatabaseSyncable<ColorVisionConfig>
{
    public required Guid PlayerId { get; set; }
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

    public static ColorVisionConfig RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}