namespace Model;

public class MatchHistoryPlayerData
{
    public required Guid PlayerId { get; set; }
    public required string NativePlatformId { get; set; }
    public required string SavedPlayerName { get; set; }
    public required string SelectedBannerCatalogId { get; set; }
    public required string SavedSponsorName { get; set; }
    public required bool IsAnonymousPlayer { get; set; }
    public required bool HasCrewScoreEarned { get; set; }
    public required int TeammateIndex { get; set; }
    public required int NumKills { get; set; }
    public required int NumAssists { get; set; }
    public required int NumDeaths { get; set; }
    public required int TotalDamageDone { get; set; }
    public required int CurrentRankId { get; set; }
    public required int PreviousRankId { get; set; }
    public required int CurrentRankedRating { get; set; }
    public required int PreviousRankedRating { get; set; }
    public required int RankedRatingDelta { get; set; }
    public required int CrewScore { get; set; }
    public required Guid CrewId { get; set; }
    public required Guid DivisionId { get; set; }
    public required int DivisionType { get; set; }
    public required string[] MatchPlacementData { get; set; }
    public required int NumRankedMatches { get; set; }
}