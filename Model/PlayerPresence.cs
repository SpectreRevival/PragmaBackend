using Npgsql;
using NpgsqlTypes;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class PlayerPresence : IDatabaseSyncableDefault<PlayerPresence, Guid>, IEquatable<PlayerPresence>
{
    [SetsRequiredMembers]
    public PlayerPresence(Guid playerId, PlayerBasicPresence basicStatus, DateTimeOffset lastUpdatedTime, int advancedPresenceType, string advancedPresenceContext)
    {
        PlayerId = playerId;
        BasicStatus = basicStatus;
        LastUpdatedTime = lastUpdatedTime;
        AdvancedPresenceType = advancedPresenceType;
        AdvancedPresenceContext = advancedPresenceContext ?? throw new ArgumentNullException(nameof(advancedPresenceContext));
    }

    public required Guid PlayerId { get; set; }
    public required PlayerBasicPresence BasicStatus { get; set; }
    public required DateTimeOffset LastUpdatedTime { get; set; }
    public required Int32 AdvancedPresenceType { get; set; } // Todo make enum
    public required string AdvancedPresenceContext { get; set; }

    public static async Task<PlayerPresence?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_player_presence.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new PlayerPresence(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<PlayerBasicPresence>(1),
            await reader.GetFieldValueAsync<DateTimeOffset>(4),
            await reader.GetFieldValueAsync<Int32>(2),
            await reader.GetFieldValueAsync<string>(3)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_player_presence.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("basic_status", BasicStatus);
        cmd.Parameters.AddWithValue("last_updated_time", LastUpdatedTime);
        cmd.Parameters.AddWithValue("advanced_presence_type", AdvancedPresenceType);
        cmd.Parameters.AddWithValue("advanced_presence_context", AdvancedPresenceContext);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(PlayerPresence? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
            && BasicStatus == other.BasicStatus
            && LastUpdatedTime.ToUnixTimeMilliseconds() == other.LastUpdatedTime.ToUnixTimeMilliseconds()
            && AdvancedPresenceType == other.AdvancedPresenceType
            && AdvancedPresenceContext == other.AdvancedPresenceContext;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(PlayerId);
        hash.Add(BasicStatus);
        hash.Add(LastUpdatedTime.ToUnixTimeMilliseconds());
        hash.Add(AdvancedPresenceType);
        hash.Add(AdvancedPresenceContext);
        return hash.ToHashCode();
    }

    public static PlayerPresence CreateDefault(Guid key)
    {
        return new PlayerPresence(key, PlayerBasicPresence.Offline, DateTimeOffset.MinValue, 0, "");
    }
}

public enum PlayerBasicPresence
{
    [PgName("Online")]
    Online = 0,
    [PgName("Offline")]
    Offline = 1
}