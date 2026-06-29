using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class LegacySeasonData : IDatabaseSyncableDefault<LegacySeasonData, Guid>, IEquatable<LegacySeasonData>, IInterchangeableKeyed<LegacySeasonData, Packets.LegacySeasonData, Guid>
{
    [SetsRequiredMembers]
    public LegacySeasonData(Guid playerId, long soloRankedPoints, long currentSoloRank)
    {
        PlayerId = playerId;
        SoloRankedPoints = soloRankedPoints;
        CurrentSoloRank = currentSoloRank;
    }

    public required Guid PlayerId { get; set; }
    public required long SoloRankedPoints { get; set; }
    public required long CurrentSoloRank { get; set; } // TODO make enum

    public static async Task<LegacySeasonData?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_legacy_season_data.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new LegacySeasonData(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<long>(1),
            await reader.GetFieldValueAsync<long>(2)
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
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId && SoloRankedPoints == other.SoloRankedPoints && CurrentSoloRank == other.CurrentSoloRank));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(SoloRankedPoints);
        hash.Add(CurrentSoloRank);
        return hash.ToHashCode();
    }

    public static LegacySeasonData CreateDefault(Guid key)
    {
        return new LegacySeasonData(key, 0, 0);
    }

    public static LegacySeasonData FromPacket(Packets.LegacySeasonData inst, Guid id)
    {
        return new LegacySeasonData(id, (long)inst.SoloRankPoints, (long)inst.CurrentSoloRank);
    }

    public Packets.LegacySeasonData ToPacket()
    {
        return new Packets.LegacySeasonData()
        {
            SoloRankPoints = SoloRankedPoints,
            CurrentSoloRank = CurrentSoloRank
        };
    }
}