namespace Model;

public abstract record class TrackedProgression
{
    public required Guid PlayerId { get; set; }
    public required Guid[] ActiveDailyQuests { get; set; }
    public required Guid[] ActiveWeeklyQuests { get; set; }
    public required Guid[] ActiveEventQuests { get; set; }
    public required DateTimeOffset LastRolloverTimestamp { get; set; }
}

public record class TeamTrackedProgression : TrackedProgression, IDatabaseSyncable<TeamTrackedProgression, Guid>
{
    public required Guid TeamId { get; set; }

    public static Task<TeamTrackedProgression?> RetrieveFromDatabase(Guid key)
    {
        throw new NotImplementedException();
    }

    public Guid GetKey()
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}

public record class IndividualTrackedProgression : TrackedProgression, IDatabaseSyncable<IndividualTrackedProgression, Guid>
{
    public required Guid ActiveEndorsement { get; set; }

    public static Task<IndividualTrackedProgression?> RetrieveFromDatabase(Guid key)
    {
        throw new NotImplementedException();
    }

    public Guid GetKey()
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}