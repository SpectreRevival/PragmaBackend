using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public enum LegacyStatsType
{
    Casual = 0,
    Ranked = 1,
    Team = 2
}

public record class LegacyStatsDataKey
{
    [SetsRequiredMembers]
    public LegacyStatsDataKey(Guid playerId, LegacyStatsType statsType)
    {
        PlayerId = playerId;
        StatsType = statsType;
    }

    public required Guid PlayerId { get; set; }
    public required LegacyStatsType StatsType { get; set; }
}

public record class LegacyStatsData : IDatabaseSyncableDefault<LegacyStatsData, LegacyStatsDataKey>, IEquatable<LegacyStatsData>
{
    [SetsRequiredMembers]
    public LegacyStatsData(Guid playerId, LegacyStatsType statsType, long killCount, long deathCount, long aceCount, long dualityKillCount, long firstKillCount, long firstDeathCount, double kAST, double dualityRating, long impactCount, long totalMatchesPlayedCount, long fanCount, long winCount, long totalRoundsPlayedCount, long headshotsCount, long totalDamagingShotsCount, long totalDamageCount, string[] topSponsors, string[] topWeapons)
    {
        PlayerId = playerId;
        StatsType = statsType;
        KillCount = killCount;
        DeathCount = deathCount;
        AceCount = aceCount;
        DualityKillCount = dualityKillCount;
        FirstKillCount = firstKillCount;
        FirstDeathCount = firstDeathCount;
        KAST = kAST;
        DualityRating = dualityRating;
        ImpactCount = impactCount;
        TotalMatchesPlayedCount = totalMatchesPlayedCount;
        FanCount = fanCount;
        WinCount = winCount;
        TotalRoundsPlayedCount = totalRoundsPlayedCount;
        HeadshotsCount = headshotsCount;
        TotalDamagingShotsCount = totalDamagingShotsCount;
        TotalDamageCount = totalDamageCount;
        TopSponsors = topSponsors ?? throw new ArgumentNullException(nameof(topSponsors));
        TopWeapons = topWeapons ?? throw new ArgumentNullException(nameof(topWeapons));
    }

    public required Guid PlayerId { get; set; }
    public required LegacyStatsType StatsType { get; set; }
    public required Int64 KillCount { get; set; } = 0;
    public required Int64 DeathCount { get; set; } = 0;
    public required Int64 AceCount { get; set; } = 0;
    public required Int64 DualityKillCount { get; set; } = 0;
    public required Int64 FirstKillCount { get; set; } = 0;
    public required Int64 FirstDeathCount { get; set; } = 0;
    public required double KAST { get; set; } = 0;
    public required double DualityRating { get; set; } = 0;
    public required Int64 ImpactCount { get; set; } = 0;
    public required Int64 TotalMatchesPlayedCount { get; set; } = 0;
    public required Int64 FanCount { get; set; } = 0;
    public required Int64 WinCount { get; set; } = 0;
    public required Int64 TotalRoundsPlayedCount { get; set; } = 0;
    public required Int64 HeadshotsCount { get; set; } = 0;
    public required Int64 TotalDamagingShotsCount { get; set; } = 0;
    public required Int64 TotalDamageCount { get; set; } = 0;
    public required string[] TopSponsors { get; set; } = [];
    public required string[] TopWeapons { get; set; } = [];

    public static async Task<LegacyStatsData?> RetrieveFromDatabase(LegacyStatsDataKey key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile($"query_legacy_stats_data_{key.StatsType.ToString().ToLower()}.sql");
        cmd.Parameters.AddWithValue("player_id", key.PlayerId);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new LegacyStatsData(
            await reader.GetFieldValueAsync<Guid>(0),
            key.StatsType,
            await reader.GetFieldValueAsync<Int64>(1),
            await reader.GetFieldValueAsync<Int64>(2),
            await reader.GetFieldValueAsync<Int64>(3),
            await reader.GetFieldValueAsync<Int64>(4),
            await reader.GetFieldValueAsync<Int64>(5),
            await reader.GetFieldValueAsync<Int64>(6),
            await reader.GetFieldValueAsync<double>(7),
            await reader.GetFieldValueAsync<double>(8),
            await reader.GetFieldValueAsync<Int64>(9),
            await reader.GetFieldValueAsync<Int64>(10),
            await reader.GetFieldValueAsync<Int64>(11),
            await reader.GetFieldValueAsync<Int64>(12),
            await reader.GetFieldValueAsync<Int64>(13),
            await reader.GetFieldValueAsync<Int64>(14),
            await reader.GetFieldValueAsync<Int64>(15),
            await reader.GetFieldValueAsync<Int64>(16),
            await reader.GetFieldValueAsync<string[]>(17),
            await reader.GetFieldValueAsync<string[]>(18)
        );
    }

    public LegacyStatsDataKey GetKey()
    {
        return new LegacyStatsDataKey(PlayerId, StatsType);
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile($"save_legacy_stats_data_{StatsType.ToString().ToLower()}.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("kill_count", KillCount);
        cmd.Parameters.AddWithValue("death_count", DeathCount);
        cmd.Parameters.AddWithValue("ace_count", AceCount);
        cmd.Parameters.AddWithValue("duality_kill_count", DualityKillCount);
        cmd.Parameters.AddWithValue("first_kill_count", FirstKillCount);
        cmd.Parameters.AddWithValue("first_death_count", FirstDeathCount);
        cmd.Parameters.AddWithValue("kast", KAST);
        cmd.Parameters.AddWithValue("duality_rating", DualityRating);
        cmd.Parameters.AddWithValue("impact_count", ImpactCount);
        cmd.Parameters.AddWithValue("total_matches_played", TotalMatchesPlayedCount);
        cmd.Parameters.AddWithValue("fan_count", FanCount);
        cmd.Parameters.AddWithValue("win_count", WinCount);
        cmd.Parameters.AddWithValue("total_rounds_played", TotalRoundsPlayedCount);
        cmd.Parameters.AddWithValue("headshots_count", HeadshotsCount);
        cmd.Parameters.AddWithValue("total_damaging_shots_count", TotalDamagingShotsCount);
        cmd.Parameters.AddWithValue("total_damage", TotalDamageCount);
        cmd.Parameters.AddWithValue("top_sponsors", TopSponsors);
        cmd.Parameters.AddWithValue("top_weapons", TopWeapons);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(LegacyStatsData? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
            && StatsType == other.StatsType
            && KillCount == other.KillCount
            && DeathCount == other.DeathCount
            && AceCount == other.AceCount
            && DualityKillCount == other.DualityKillCount
            && FirstKillCount == other.FirstKillCount
            && FirstDeathCount == other.FirstDeathCount
            && KAST == other.KAST
            && DualityRating == other.DualityRating
            && ImpactCount == other.ImpactCount
            && TotalMatchesPlayedCount == other.TotalMatchesPlayedCount
            && FanCount == other.FanCount
            && WinCount == other.WinCount
            && TotalRoundsPlayedCount == other.TotalRoundsPlayedCount
            && HeadshotsCount == other.HeadshotsCount
            && TotalDamagingShotsCount == other.TotalDamagingShotsCount
            && TotalDamageCount == other.TotalDamageCount
            && TopSponsors.SequenceEqual(other.TopSponsors)
            && TopWeapons.SequenceEqual(other.TopWeapons);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(PlayerId);
        hash.Add(StatsType);
        hash.Add(KillCount);
        hash.Add(DeathCount);
        hash.Add(AceCount);
        hash.Add(DualityKillCount);
        hash.Add(FirstKillCount);
        hash.Add(FirstDeathCount);
        hash.Add(KAST);
        hash.Add(DualityRating);
        hash.Add(ImpactCount);
        hash.Add(TotalMatchesPlayedCount);
        hash.Add(FanCount);
        hash.Add(WinCount);
        hash.Add(TotalRoundsPlayedCount);
        hash.Add(HeadshotsCount);
        hash.Add(TotalDamagingShotsCount);
        hash.Add(TotalDamageCount);
        hash.Add(TopSponsors);
        hash.Add(TopWeapons);
        return hash.ToHashCode();
    }

    public static LegacyStatsData CreateDefault(LegacyStatsDataKey key)
    {
        return new LegacyStatsData(key.PlayerId, key.StatsType, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, [], []);
    }
}