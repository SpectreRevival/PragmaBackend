namespace Model;

public abstract record class TrackedProgression
{
    public required string[] ActiveDailyQuests { get; set; }
    public required string[] ActiveWeeklyQuests { get; set; }
    public required string[] ActiveEventQuests { get; set; }
    public required DateTimeOffset LastRolloverTimestamp { get; set; }
}

public record class TeamTrackedProgression : TrackedProgression
{
    public required string TeamId { get; set; }
}

public record class IndividualTrackedProgression : TrackedProgression
{
    public required string ActiveEndorsement { get; set; }
}