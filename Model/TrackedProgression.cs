namespace Model;

public abstract record class TrackedProgression
{
    public required Guid[] ActiveDailyQuests { get; set; }
    public required Guid[] ActiveWeeklyQuests { get; set; }
    public required Guid[] ActiveEventQuests { get; set; }
    public required DateTimeOffset LastRolloverTimestamp { get; set; }
}

public record class TeamTrackedProgression : TrackedProgression
{
    public required Guid TeamId { get; set; }
}

public record class IndividualTrackedProgression : TrackedProgression
{
    public required Guid ActiveEndorsement { get; set; }
}