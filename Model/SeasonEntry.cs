namespace Model;

public record class SeasonEntry
{
    public required Int32 SeasonNumber { get; set; }
    public required DateTimeOffset StartTimestampMillis { get; set; }
    public required DateTimeOffset EndTimestampMillis { get; set; }
    public required DateTimeOffset LastWeekEndTimestmapMillis { get; set; }
    public required Int32 NumberOfWeeksInSeason { get; set; }
}