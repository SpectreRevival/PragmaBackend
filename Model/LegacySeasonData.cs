using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class LegacySeasonData : IDatabaseSyncableDefault<LegacySeasonData, Guid>, IEquatable<LegacySeasonData>
{
    [SetsRequiredMembers]
    public LegacySeasonData(Guid playerId, long soloRankedPoints, long currentSoloRank)
    {
        PlayerId = playerId;
        SoloRankedPoints = soloRankedPoints;
        CurrentSoloRank = currentSoloRank;
    }

    public required Guid PlayerId { get; set; }
    public required Int64 SoloRankedPoints { get; set; }
    public required Int64 CurrentSoloRank { get; set; } // TODO make enum

    public static async Task<LegacySeasonData?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_legacy_season_data.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new LegacySeasonData(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<Int64>(1),
            await reader.GetFieldValueAsync<Int64>(2)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_legacy_season_data.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("solo_ranked_points", SoloRankedPoints);
        cmd.Parameters.AddWithValue("current_solo_rank", CurrentSoloRank);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(LegacySeasonData? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
            && SoloRankedPoints == other.SoloRankedPoints
            && CurrentSoloRank == other.CurrentSoloRank;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(PlayerId);
        hash.Add(SoloRankedPoints);
        hash.Add(CurrentSoloRank);
        return hash.ToHashCode();
    }

    public static LegacySeasonData CreateDefault(Guid key)
    {
        return new LegacySeasonData(key, 0, 0);
    }
}