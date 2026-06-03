namespace Model;

public record class LegacyStatsData : IDatabaseSyncable<LegacyStatsData>
{
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

    public static Task<LegacyStatsData?> RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }

    public enum LegacyStatsType
    {
        Casual = 0,
        Ranked = 1,
        Team = 2
    }
}