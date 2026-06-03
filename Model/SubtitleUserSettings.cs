namespace Model;

public record class SubtitleUserSettings : VersionedData, IDatabaseSyncable<SubtitleUserSettings>
{
    public required Guid PlayerId { get; set; }
    public required Int32 FontSize { get; set; }
    public required double BackgroundOpacity { get; set; }
    public required string SpeakerQualifierDisplay { get; set; }
    public required bool PostPlayerSubtitles { get; set; }
    public required bool PostPlayerSubtitlesToChat { get; set; }
    public required Int32 NamesToShowMask { get; set; }

    public static SubtitleUserSettings RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}