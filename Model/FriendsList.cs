using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class FriendsList : VersionedData, IDatabaseSyncable<FriendsList, Guid>, IEquatable<FriendsList>
{
    [SetsRequiredMembers]
    public FriendsList(Guid playerId, bool acceptingFriendInvites, Guid[] friends, Guid[] blocked, Guid[] sentFriendInvites, Guid[] receivedFriendInvites, Int64 version)
    {
        PlayerId = playerId;
        AcceptingFriendInvites = acceptingFriendInvites;
        Friends = friends ?? throw new ArgumentNullException(nameof(friends));
        Blocked = blocked ?? throw new ArgumentNullException(nameof(blocked));
        SentFriendInvites = sentFriendInvites ?? throw new ArgumentNullException(nameof(sentFriendInvites));
        ReceivedFriendInvites = receivedFriendInvites ?? throw new ArgumentNullException(nameof(receivedFriendInvites));
        Version = version;
    }

    public required Guid PlayerId { get; set; }
    public required bool AcceptingFriendInvites { get; set; } = true;
    public required Guid[] Friends { get; set; } = [];
    public required Guid[] Blocked { get; set; } = [];
    public required Guid[] SentFriendInvites { get; set; } = [];
    public required Guid[] ReceivedFriendInvites { get; set; } = [];

    public static async Task<FriendsList?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_friends_list.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new FriendsList(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<bool>(1),
            await reader.GetFieldValueAsync<Guid[]>(2),
            await reader.GetFieldValueAsync<Guid[]>(3),
            await reader.GetFieldValueAsync<Guid[]>(4),
            await reader.GetFieldValueAsync<Guid[]>(5),
            await reader.GetFieldValueAsync<Int64>(6)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_friends_list.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("accepting_friend_invites", AcceptingFriendInvites);
        cmd.Parameters.AddWithValue("friends", Friends);
        cmd.Parameters.AddWithValue("blocked", Blocked);
        cmd.Parameters.AddWithValue("sent_friend_invites", SentFriendInvites);
        cmd.Parameters.AddWithValue("received_friend_invites", ReceivedFriendInvites);
        cmd.Parameters.AddWithValue("version", Version);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(FriendsList? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
            && AcceptingFriendInvites == other.AcceptingFriendInvites
            && Friends.SequenceEqual(other.Friends)
            && Blocked.SequenceEqual(other.Blocked)
            && SentFriendInvites.SequenceEqual(other.SentFriendInvites)
            && ReceivedFriendInvites.SequenceEqual(other.ReceivedFriendInvites)
            && Version == other.Version;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(PlayerId);
        hash.Add(AcceptingFriendInvites);
        hash.Add(Friends);
        hash.Add(Blocked);
        hash.Add(SentFriendInvites);
        hash.Add(ReceivedFriendInvites);
        hash.Add(Version);
        return hash.ToHashCode();
    }
}