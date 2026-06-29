using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class Party : VersionedData, IDatabaseSyncable<Party, Guid>, IEquatable<Party>
{
    [SetsRequiredMembers]
    public Party(Guid partyId, PartyMember[] members, string inviteCode, string queuePool, string lobbyMode, string chatId, bool useTeamMMR, long version) : base(version)
    {
        PartyId = partyId;
        Members = members ?? throw new ArgumentNullException(nameof(members));
        InviteCode = inviteCode ?? throw new ArgumentNullException(nameof(inviteCode));
        QueuePool = queuePool ?? throw new ArgumentNullException(nameof(queuePool));
        LobbyMode = lobbyMode ?? throw new ArgumentNullException(nameof(lobbyMode));
        ChatId = chatId ?? throw new ArgumentNullException(nameof(chatId));
        UseTeamMMR = useTeamMMR;
    }

    public required Guid PartyId { get; set; }
    public required PartyMember[] Members { get; set; }
    public required string InviteCode { get; set; }
    public required string QueuePool { get; set; }
    public required string LobbyMode { get; set; }
    public required string ChatId { get; set; }
    public required bool UseTeamMMR { get; set; }

    public static async Task<Party?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_party.sql");
        cmd.Parameters.AddWithValue("party_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new Party(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<PartyMember[]>(1),
            await reader.GetFieldValueAsync<string>(2),
            await reader.GetFieldValueAsync<string>(3),
            await reader.GetFieldValueAsync<string>(4),
            await reader.GetFieldValueAsync<string>(5),
            await reader.GetFieldValueAsync<bool>(6),
            await reader.GetFieldValueAsync<long>(7)
        );
    }

    public Guid GetKey()
    {
        return PartyId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_party.sql");
        cmd.Parameters.AddWithValue("party_id", PartyId);
        cmd.Parameters.AddWithValue("members", Members);
        cmd.Parameters.AddWithValue("invite_code", InviteCode);
        cmd.Parameters.AddWithValue("queue_pool", QueuePool);
        cmd.Parameters.AddWithValue("lobby_mode", LobbyMode);
        cmd.Parameters.AddWithValue("chat_id", ChatId);
        cmd.Parameters.AddWithValue("use_team_mmr", UseTeamMMR);
        cmd.Parameters.AddWithValue("version", Version);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(Party? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (PartyId == other.PartyId
            && Members.SequenceEqual(other.Members)
            && InviteCode == other.InviteCode
            && QueuePool == other.QueuePool
            && LobbyMode == other.LobbyMode
            && ChatId == other.ChatId
            && UseTeamMMR == other.UseTeamMMR
            && Version == other.Version));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PartyId);
        hash.Add(Members);
        hash.Add(InviteCode);
        hash.Add(QueuePool);
        hash.Add(LobbyMode);
        hash.Add(ChatId);
        hash.Add(UseTeamMMR);
        hash.Add(Version);
        return hash.ToHashCode();
    }
}