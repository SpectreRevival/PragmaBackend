using Model.Persistence;
using Npgsql;
using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class PlayerPresence : IDatabaseSyncableDefault<PlayerPresence, Guid>, IEquatable<PlayerPresence>
{
    private static readonly PlayerPresence defaultData = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "PlayerPresence.json")))
        .Deserialize<PlayerPresence>(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(), new UnixDateTimeOffsetConverter() }
        });

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
    public required int AdvancedPresenceType { get; set; } // Todo make enum
    public required string AdvancedPresenceContext { get; set; }

    public static async Task<PlayerPresence?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_player_presence.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new PlayerPresence(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<PlayerBasicPresence>(1),
            await reader.GetFieldValueAsync<DateTimeOffset>(4),
            await reader.GetFieldValueAsync<int>(2),
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
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId
            && BasicStatus == other.BasicStatus
            && LastUpdatedTime.ToUnixTimeMilliseconds() == other.LastUpdatedTime.ToUnixTimeMilliseconds()
            && AdvancedPresenceType == other.AdvancedPresenceType
            && AdvancedPresenceContext == other.AdvancedPresenceContext));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(BasicStatus);
        hash.Add(LastUpdatedTime.ToUnixTimeMilliseconds());
        hash.Add(AdvancedPresenceType);
        hash.Add(AdvancedPresenceContext);
        return hash.ToHashCode();
    }

    public static PlayerPresence CreateDefault(Guid playerId)
    {
        return defaultData with { PlayerId = playerId };
    }
}

public enum PlayerBasicPresence
{
    [PgName("Online")]
    Online = 0,
    [PgName("Offline")]
    Offline = 1
}