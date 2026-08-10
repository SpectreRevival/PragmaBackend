using Model.Persistence;
using Npgsql;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public class MatchHistoryTeamData : IEquatable<MatchHistoryTeamData?>
{
    [SetsRequiredMembers]
    public MatchHistoryTeamData(int roundsPlayed, int roundsWon, int xpPerRound, int xpPerRoundWon, Guid teamId, int currentRankId, int previousRankId, int currentRankedRating, int previousRankedRating, int rankedRatingDelta, string[] matchPlacementData, int numRankedMatches, int fansPerRound, int fansPerRoundWon, MatchHistoryPlayerData[] playerData, bool usedTeamRank, bool isFullTeamInParty)
    {
        RoundsPlayed = roundsPlayed;
        RoundsWon = roundsWon;
        XpPerRound = xpPerRound;
        XpPerRoundWon = xpPerRoundWon;
        TeamId = teamId;
        CurrentRankId = currentRankId;
        PreviousRankId = previousRankId;
        CurrentRankedRating = currentRankedRating;
        PreviousRankedRating = previousRankedRating;
        RankedRatingDelta = rankedRatingDelta;
        MatchPlacementData = matchPlacementData ?? throw new ArgumentNullException(nameof(matchPlacementData));
        NumRankedMatches = numRankedMatches;
        FansPerRound = fansPerRound;
        FansPerRoundWon = fansPerRoundWon;
        PlayerData = playerData ?? throw new ArgumentNullException(nameof(playerData));
        UsedTeamRank = usedTeamRank;
        IsFullTeamInParty = isFullTeamInParty;
    }

    public required int RoundsPlayed { get; set; }
    public required int RoundsWon { get; set; }
    public required int XpPerRound { get; set; }
    public required int XpPerRoundWon { get; set; }
    public int XpGained => XpPerRoundWon * RoundsWon + XpPerRound * RoundsPlayed;
    /* not the team ID in the sense of which team in the match, but in the sense of team ranks */
    public required Guid TeamId { get; set; }
    /** Only for team ranks, if not used, this will be 0 */
    public required int CurrentRankId { get; set; }
    /** Only for team ranks, if not used, this will be 0 */
    public required int PreviousRankId { get; set; }
    /** Only for team ranks, if not used, this will be 0 */
    public required int CurrentRankedRating { get; set; }
    /** Only for team ranks, if not used, this will be 0 */
    public required int PreviousRankedRating { get; set; }
    /** Only for team ranks, if not used, this will be 0 */
    public required int RankedRatingDelta { get; set; }
    /** Only for team ranks, if not used, this will be empty */
    public required string[] MatchPlacementData { get; set; }
    /** Only for team ranks, if not used, this will be 0 */
    public required int NumRankedMatches { get; set; }
    public required int FansPerRound { get; set; }
    public required int FansPerRoundWon { get; set; }
    public int FansGained => FansPerRoundWon * RoundsWon + FansPerRound * RoundsPlayed;
    public required MatchHistoryPlayerData[] PlayerData { get; set; }
    public required bool UsedTeamRank { get; set; }
    public required bool IsFullTeamInParty { get; set; }

    /** Does not handle player data */
    private NpgsqlBatchCommand CreateBatchSync(Guid matchId, int teamNumber)
    {
        var cmd = PostgresDatabase.LoadBatchCommandFromFile("save/team_match_history.sql");
        cmd.Parameters.AddWithValue("match_id", matchId);
        cmd.Parameters.AddWithValue("team_number", teamNumber);
        cmd.Parameters.AddWithValue("rounds_played", RoundsPlayed);
        cmd.Parameters.AddWithValue("rounds_won", RoundsWon);
        cmd.Parameters.AddWithValue("xp_per_round", XpPerRound);
        cmd.Parameters.AddWithValue("xp_per_round_won", XpPerRoundWon);
        cmd.Parameters.AddWithValue("team_id", TeamId);
        cmd.Parameters.AddWithValue("current_rank_id", CurrentRankId);
        cmd.Parameters.AddWithValue("previous_rank_id", PreviousRankId);
        cmd.Parameters.AddWithValue("current_ranked_rating", CurrentRankedRating);
        cmd.Parameters.AddWithValue("previous_ranked_rating", PreviousRankedRating);
        cmd.Parameters.AddWithValue("ranked_rating_delta", RankedRatingDelta);
        cmd.Parameters.AddWithValue("match_placement_data", MatchPlacementData);
        cmd.Parameters.AddWithValue("num_ranked_matches", NumRankedMatches);
        cmd.Parameters.AddWithValue("fans_per_round", FansPerRound);
        cmd.Parameters.AddWithValue("fans_per_round_won", FansPerRoundWon);
        cmd.Parameters.AddWithValue("used_team_rank", UsedTeamRank);
        cmd.Parameters.AddWithValue("is_full_team_in_party", IsFullTeamInParty);
        return cmd;
    }

    public void AddSyncToBatch(NpgsqlBatch batch, Guid matchId, int teamNumber)
    {
        foreach(var cmd in GetBatchCommands(matchId, teamNumber))
        {
            batch.BatchCommands.Add(cmd);
        }
    }

    public List<NpgsqlBatchCommand> GetBatchCommands(Guid matchId, int teamNumber)
    {
        var commands = new List<NpgsqlBatchCommand>();
        commands.Add(CreateBatchSync(matchId, teamNumber));
        for (int i = 0; i < PlayerData.Length; i++)
        {
            commands.Add(PlayerData[i].CreateSyncCommand(teamNumber, matchId));
        }
        return commands;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as MatchHistoryTeamData);
    }

    public bool Equals(MatchHistoryTeamData? other)
    {
        return other is not null &&
               RoundsPlayed == other.RoundsPlayed &&
               RoundsWon == other.RoundsWon &&
               XpPerRound == other.XpPerRound &&
               XpPerRoundWon == other.XpPerRoundWon &&
               XpGained == other.XpGained &&
               TeamId.Equals(other.TeamId) &&
               CurrentRankId == other.CurrentRankId &&
               PreviousRankId == other.PreviousRankId &&
               CurrentRankedRating == other.CurrentRankedRating &&
               PreviousRankedRating == other.PreviousRankedRating &&
               RankedRatingDelta == other.RankedRatingDelta &&
               EqualityComparer<string[]>.Default.Equals(MatchPlacementData, other.MatchPlacementData) &&
               NumRankedMatches == other.NumRankedMatches &&
               FansPerRound == other.FansPerRound &&
               FansPerRoundWon == other.FansPerRoundWon &&
               FansGained == other.FansGained &&
               EqualityComparer<MatchHistoryPlayerData[]>.Default.Equals(PlayerData, other.PlayerData) &&
               UsedTeamRank == other.UsedTeamRank &&
               IsFullTeamInParty == other.IsFullTeamInParty;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(RoundsPlayed);
        hash.Add(RoundsWon);
        hash.Add(XpPerRound);
        hash.Add(XpPerRoundWon);
        hash.Add(XpGained);
        hash.Add(TeamId);
        hash.Add(CurrentRankId);
        hash.Add(PreviousRankId);
        hash.Add(CurrentRankedRating);
        hash.Add(PreviousRankedRating);
        hash.Add(RankedRatingDelta);
        hash.Add(MatchPlacementData);
        hash.Add(NumRankedMatches);
        hash.Add(FansPerRound);
        hash.Add(FansPerRoundWon);
        hash.Add(FansGained);
        hash.Add(PlayerData);
        hash.Add(UsedTeamRank);
        hash.Add(IsFullTeamInParty);
        return hash.ToHashCode();
    }

    public static bool operator ==(MatchHistoryTeamData? left, MatchHistoryTeamData? right)
    {
        return EqualityComparer<MatchHistoryTeamData>.Default.Equals(left, right);
    }

    public static bool operator !=(MatchHistoryTeamData? left, MatchHistoryTeamData? right)
    {
        return !(left == right);
    }
}