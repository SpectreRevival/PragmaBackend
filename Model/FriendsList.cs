namespace Model;

public record class FriendsList : VersionedData
{
    public required bool AcceptingFriendInvites { get; set; } = true;
    public required string[] Friends { get; set; } = [];
    public required string[] Blocked { get; set; } = [];
    public required string[] SentFriendInvites { get; set; } = [];
    public required string[] ReceivedFriendInvites { get; set; } = [];
}