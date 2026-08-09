namespace Model;

public class MatchHistoryTeamData
{
    public required int RoundsPlayed { get; set; }
    public required int RoundsWon { get; set; }
    public required int XpPerRound { get; set; }
    public required int XpPerRoundWon { get; set; }
    public int XpGained => XpPerRoundWon * RoundsWon + XpPerRound * RoundsPlayed;
    // not the team ID in the sense of which team in the match, but in the sense of team ranks
    public required Guid TeamId { get; set; }
    public required int CurrentRankId { get; set; }
    public required int PreviousRankId { get; set; }
    public required int CurrentRankedRating { get; set; }
    public required int PreviousRankedRating { get; set; }
    public required int RankedRatingDelta { get; set; }
    public required string[] MatchPlacementData { get; set; }
    public required int NumRankedMatches { get; set; }
    public required int FansPerRound { get; set; }
    public required int FansPerRoundWon { get; set; }
    public int FansGained => FansPerRoundWon * RoundsWon + FansPerRound * RoundsPlayed;
    public required MatchHistoryPlayerData[] PlayerData { get; set; }
    public required bool UsedTeamRank { get; set; }
    public required bool IsFullTeamInParty { get; set; }
}