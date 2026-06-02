namespace Model;

public record class LegacyPlayerData
{
    public required LegacySeasonData SeasonData { get; set; }
    public required LegacyStatsData RankedStats { get; set; }
    public required LegacyStatsData CasualStats { get; set; }
    public required LegacyStatsData TeamStats { get; set; }
}