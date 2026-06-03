namespace Model;

public abstract record class TrackedProgression
{
    public required Guid PlayerId { get; set; }
    public required Guid[] ActiveDailyQuests { get; set; }
    public required Guid[] ActiveWeeklyQuests { get; set; }
    public required Guid[] ActiveEventQuests { get; set; }
    public required DateTimeOffset LastRolloverTimestamp { get; set; }
}

public record class TeamTrackedProgression : TrackedProgression, IDatabaseSyncable<TeamTrackedProgression>
{
    public required Guid TeamId { get; set; }

    public static TeamTrackedProgression RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}

public record class IndividualTrackedProgression : TrackedProgression, IDatabaseSyncable<IndividualTrackedProgression>
{
    public required Guid ActiveEndorsement { get; set; }

    public static IndividualTrackedProgression RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}