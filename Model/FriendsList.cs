namespace Model;

public record class FriendsList : VersionedData
{
    public required bool AcceptingFriendInvites { get; set; } = true;
    public required Guid[] Friends { get; set; } = [];
    public required Guid[] Blocked { get; set; } = [];
    public required Guid[] SentFriendInvites { get; set; } = [];
    public required Guid[] ReceivedFriendInvites { get; set; } = [];
}