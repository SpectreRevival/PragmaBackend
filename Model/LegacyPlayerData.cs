namespace Model;

public record class LegacyPlayerData : IDatabaseSyncable<LegacyPlayerData>
{
    public required Guid PlayerId { get; set; }
    public required LegacySeasonData SeasonData { get; set; }
    public required LegacyStatsData RankedStats { get; set; }
    public required LegacyStatsData CasualStats { get; set; }
    public required LegacyStatsData TeamStats { get; set; }

    public static LegacyPlayerData RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}