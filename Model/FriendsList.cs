namespace Model;

public record class FriendsList : VersionedData, IDatabaseSyncable<FriendsList>
{
    public required Guid PlayerId { get; set; }
    public required bool AcceptingFriendInvites { get; set; } = true;
    public required Guid[] Friends { get; set; } = [];
    public required Guid[] Blocked { get; set; } = [];
    public required Guid[] SentFriendInvites { get; set; } = [];
    public required Guid[] ReceivedFriendInvites { get; set; } = [];

    public static FriendsList RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}