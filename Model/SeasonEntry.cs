namespace Model;

public record class SeasonEntry : IDatabaseSyncable<SeasonEntry>
{
    public required Int32 SeasonNumber { get; set; }
    public required DateTimeOffset StartTimestampMillis { get; set; }
    public required DateTimeOffset EndTimestampMillis { get; set; }
    public required DateTimeOffset LastWeekEndTimestmapMillis { get; set; }
    public required Int32 NumberOfWeeksInSeason { get; set; }

    public static SeasonEntry RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}