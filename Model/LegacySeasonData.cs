namespace Model;

public record class LegacySeasonData : IDatabaseSyncable<LegacySeasonData, Guid>
{
    public required Guid PlayerId { get; set; }
    public required Int64 SoloRankedPoints { get; set; }
    public required Int64 CurrentSoloRank { get; set; } // TODO make enum

    public static Task<LegacySeasonData?> RetrieveFromDatabase(Guid key)
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