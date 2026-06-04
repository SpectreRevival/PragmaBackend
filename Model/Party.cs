namespace Model;

public record class Party : VersionedData, IDatabaseSyncable<Party, Guid>
{
    public required Guid PartyId { get; set; }
    public required PartyMember[] Members { get; set; }
    public required string InviteCode { get; set; }
    public required string QueuePool { get; set; }
    public required string LobbyMode { get; set; }
    public required string ChatId { get; set; }
    public required bool UseTeamMMR { get; set; }

    public static Task<Party?> RetrieveFromDatabase(Guid key)
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