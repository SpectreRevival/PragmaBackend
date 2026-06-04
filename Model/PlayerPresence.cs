namespace Model;

public record class PlayerPresence : IDatabaseSyncable<PlayerPresence, Guid>
{
    public required Guid PlayerId { get; set; }
    public required PlayerBasicPresence BasicStatus { get; set; }
    public required DateTimeOffset LastUpdatedTime { get; set; }
    public required Int32 AdvancedPresenceType { get; set; } // Todo make enum
    public required string AdvancedPresenceContext { get; set; }

    public static Task<PlayerPresence?> RetrieveFromDatabase(Guid key)
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

public enum PlayerBasicPresence
{
    Online = 0,
    Offline = 1
}