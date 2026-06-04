namespace Model;

public record class LegacyPlayerData : IDatabaseSyncable<LegacyPlayerData, Guid>
{
    public required Guid PlayerId { get; set; }
    public required LegacySeasonData SeasonData { get; set; }
    public required LegacyStatsData RankedStats { get; set; }
    public required LegacyStatsData CasualStats { get; set; }
    public required LegacyStatsData TeamStats { get; set; }

    public static Task<LegacyPlayerData?> RetrieveFromDatabase(Guid key)
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