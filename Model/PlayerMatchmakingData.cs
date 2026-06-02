namespace Model;

// Ohm - Intentionally removed country, subdivision, primary/secondary geographic region and address fields from this struct since we aren't going to store those for privacy reasons.

public record class PlayerMatchmakingData
{
    public required Guid PlayerId { get; set; }
    public required double CasualMMR { get; set; } = 0.0d;
    public required double RankedMMR { get; set; } = 0.0d;
    public required Int64 SoloRankPoints { get; set; } = 0;
    public required Int64 CasualMatchesPlayed { get; set; } = 0;
    public required Int64 RankedMatchedPlayed { get; set; } = 0;
    public required Int64 CasualMatchesPlayedSeasonal { get; set; } = 0;
    public required Int64 RankedMatchesPlayedSeasonal { get; set; } = 0;
    public required string[] RankedPlacementMatches { get; set; } = [];
    public required Int64 CurrentSoloRank { get; set; } = 0;// Todo replace with enum
    public required Int64 HighestTeamRank { get; set; } = 0;// same
    public required Int64 CasualMatchesWon { get; set; } = 0;
    public required Int64 RankedMatchesWon { get; set; } = 0;
    public required DateTimeOffset PriorityMatchmakingUntil { get; set; } = DateTimeOffset.MinValue;
    public required DateTimeOffset RestrictMatchmakingUntil { get; set; } = DateTimeOffset.MinValue;
    public required string MapHistory { get; set; } = "";
}