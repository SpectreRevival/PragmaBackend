using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class FriendsList : VersionedData, IDatabaseSyncableDefault<FriendsList, Guid>, IEquatable<FriendsList>, IInterchangeableKeyed<FriendsList, Packets.FriendsList, Guid>
{
    [SetsRequiredMembers]
    public FriendsList(Guid playerId, bool acceptingFriendInvites, Guid[] friends, Guid[] blocked, Guid[] sentFriendInvites, Guid[] receivedFriendInvites, long version) : base(version)
    {
        PlayerId = playerId;
        AcceptingFriendInvites = acceptingFriendInvites;
        Friends = friends ?? throw new ArgumentNullException(nameof(friends));
        Blocked = blocked ?? throw new ArgumentNullException(nameof(blocked));
        SentFriendInvites = sentFriendInvites ?? throw new ArgumentNullException(nameof(sentFriendInvites));
        ReceivedFriendInvites = receivedFriendInvites ?? throw new ArgumentNullException(nameof(receivedFriendInvites));
    }

    public required Guid PlayerId { get; set; }
    public required bool AcceptingFriendInvites { get; set; }
    public required Guid[] Friends { get; set; }
    public required Guid[] Blocked { get; set; }
    public required Guid[] SentFriendInvites { get; set; }
    public required Guid[] ReceivedFriendInvites { get; set; }

    public static async Task<FriendsList?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_friends_list.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new FriendsList(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<bool>(1),
            await reader.GetFieldValueAsync<Guid[]>(2),
            await reader.GetFieldValueAsync<Guid[]>(3),
            await reader.GetFieldValueAsync<Guid[]>(4),
            await reader.GetFieldValueAsync<Guid[]>(5),
            await reader.GetFieldValueAsync<long>(6)
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
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId
            && AcceptingFriendInvites == other.AcceptingFriendInvites
            && Friends.SequenceEqual(other.Friends)
            && Blocked.SequenceEqual(other.Blocked)
            && SentFriendInvites.SequenceEqual(other.SentFriendInvites)
            && ReceivedFriendInvites.SequenceEqual(other.ReceivedFriendInvites)
            && Version == other.Version));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(AcceptingFriendInvites);
        hash.Add(Friends);
        hash.Add(Blocked);
        hash.Add(SentFriendInvites);
        hash.Add(ReceivedFriendInvites);
        hash.Add(Version);
        return hash.ToHashCode();
    }

    public static FriendsList CreateDefault(Guid key)
    {
        return new FriendsList(key, true, [], [], [], [], 0);
    }

    public static FriendsList FromPacket(Packets.FriendsList inst, Guid id)
    {
        return id == null
            ? throw new ArgumentNullException(nameof(id))
            : new FriendsList(id, inst.AcceptNewFriendInvites, inst.Friends.Select(id => Guid.Parse(id)).ToArray(), inst.Blocked.Select(id => Guid.Parse(id)).ToArray(), inst.SentInvites.Select(id => Guid.Parse(id)).ToArray(), inst.ReceivedInvites.Select(id => Guid.Parse(id)).ToArray(), long.Parse(inst.Version));
    }

    public Packets.FriendsList ToPacket()
    {
        Packets.FriendsList packet = new()
        {
            AcceptNewFriendInvites = AcceptingFriendInvites,
            Version = Version.ToString()
        };

        foreach (Guid friend in Friends)
        {
            packet.Friends.Add(friend.ToString());
        }
        foreach (Guid blocked in Blocked)
        {
            packet.Blocked.Add(blocked.ToString());
        }
        foreach (Guid sent in SentFriendInvites)
        {
            packet.SentInvites.Add(sent.ToString());
        }
        foreach (Guid recv in ReceivedFriendInvites)
        {
            packet.ReceivedInvites.Add(recv.ToString());
        }
        return packet;
    }
}