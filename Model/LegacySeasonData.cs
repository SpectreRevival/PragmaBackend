namespace Model;

public record class LegacySeasonData : IDatabaseSyncable<LegacySeasonData>
{
    public required Guid PlayerId { get; set; }
    public required Int64 SoloRankedPoints { get; set; }
    public required Int64 CurrentSoloRank { get; set; } // TODO make enum

    public static LegacySeasonData RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}