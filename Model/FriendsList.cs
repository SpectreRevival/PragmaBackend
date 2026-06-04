namespace Model;

public record class FriendsList : VersionedData, IDatabaseSyncable<FriendsList, Guid>
{
    public required Guid PlayerId { get; set; }
    public required bool AcceptingFriendInvites { get; set; } = true;
    public required Guid[] Friends { get; set; } = [];
    public required Guid[] Blocked { get; set; } = [];
    public required Guid[] SentFriendInvites { get; set; } = [];
    public required Guid[] ReceivedFriendInvites { get; set; } = [];

    public static Task<FriendsList?> RetrieveFromDatabase(Guid key)
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