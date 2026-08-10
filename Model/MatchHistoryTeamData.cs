using Model.Persistence;
using Npgsql;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class MatchHistoryTeamData
{
    [SetsRequiredMembers]
    public MatchHistoryTeamData(Guid matchId, int teamNumber, int roundsPlayed, int roundsWon, int xpPerRound, int xpPerRoundWon, Guid teamId, int currentRankId, int previousRankId, int currentRankedRating, int previousRankedRating, int rankedRatingDelta, string[] matchPlacementData, int numRankedMatches, int fansPerRound, int fansPerRoundWon, MatchHistoryPlayerData[] playerData, bool usedTeamRank, bool isFullTeamInParty)
    {
        MatchId = matchId;
        TeamNumber = teamNumber;
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

    public required Guid MatchId { get; set; }
    public required int TeamNumber { get; set; }
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
    private NpgsqlBatchCommand CreateBatchSync()
    {
        var cmd = PostgresDatabase.LoadBatchCommandFromFile("save/team_match_history.sql");
        cmd.Parameters.AddWithValue("match_id", MatchId);
        cmd.Parameters.AddWithValue("team_number", TeamNumber);
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
        foreach(var cmd in GetBatchCommands())
        {
            batch.BatchCommands.Add(cmd);
        }
    }

    public List<NpgsqlBatchCommand> GetBatchCommands()
    {
        var commands = new List<NpgsqlBatchCommand>();
        for (int i = 0; i < PlayerData.Length; i++)
        {
            commands.Add(PlayerData[i].CreateSyncCommand());
        }
        commands.Add(CreateBatchSync());
        return commands;
    }

    internal static MatchHistoryTeamData GetFromReader(NpgsqlDataReader reader, MatchHistoryPlayerData[]? playerData = null)
    {
        return new MatchHistoryTeamData(
            matchId: reader.GetGuid(0),
            teamNumber: reader.GetInt32(1),
            roundsPlayed: reader.GetInt32(2),
            roundsWon: reader.GetInt32(3),
            xpPerRound: reader.GetInt32(4),
            xpPerRoundWon: reader.GetInt32(5),
            teamId: reader.GetGuid(6),
            currentRankId: reader.GetInt32(7),
            previousRankId: reader.GetInt32(8),
            currentRankedRating: reader.GetInt32(9),
            previousRankedRating: reader.GetInt32(10),
            rankedRatingDelta: reader.GetInt32(11),
            matchPlacementData: reader.GetFieldValue<string[]>(12),
            numRankedMatches: reader.GetInt32(13),
            fansPerRound: reader.GetInt32(14),
            fansPerRoundWon: reader.GetInt32(15),
            playerData: playerData ?? Array.Empty<MatchHistoryPlayerData>(),
            usedTeamRank: reader.GetBoolean(16),
            isFullTeamInParty: reader.GetBoolean(17)
        );
    }

    public static async Task<MatchHistoryTeamData?> RetrieveFromDatabase(Guid matchId, int teamNumber)
    {
        var cmd = PostgresDatabase.LoadCommandFromFile("query/team_match_history.sql");
        cmd.Parameters.AddWithValue("match_id", matchId);
        cmd.Parameters.AddWithValue("team_number", teamNumber);
        using var reader = await cmd.ExecuteReaderAsync();
        if(!await reader.ReadAsync())
        {
            return null;
        }
        var ret = GetFromReader(reader);
        ret.PlayerData = await MatchHistoryPlayerData.RetrieveFromDatabase(matchId, teamNumber);
        return ret;
    }

    public static async Task<MatchHistoryTeamData[]> RetrieveFromDatabase(Guid matchId)
    {
        var cmd = PostgresDatabase.LoadCommandFromFile("query/team_match_history_all_match.sql");
        cmd.Parameters.AddWithValue("match_id", matchId);
        using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<MatchHistoryTeamData>();
        while (await reader.ReadAsync())
        {
            var res = GetFromReader(reader);
            res.PlayerData = await MatchHistoryPlayerData.RetrieveFromDatabase(matchId, res.TeamNumber);
            results.Add(res);
        }
        return results.ToArray();
    }

    public virtual bool Equals(MatchHistoryTeamData? data)
    {
        return data is not null &&
               EqualityComparer<Type>.Default.Equals(EqualityContract, data.EqualityContract) &&
               MatchId.Equals(data.MatchId) &&
               TeamNumber == data.TeamNumber &&
               RoundsPlayed == data.RoundsPlayed &&
               RoundsWon == data.RoundsWon &&
               XpPerRound == data.XpPerRound &&
               XpPerRoundWon == data.XpPerRoundWon &&
               XpGained == data.XpGained &&
               TeamId.Equals(data.TeamId) &&
               CurrentRankId == data.CurrentRankId &&
               PreviousRankId == data.PreviousRankId &&
               CurrentRankedRating == data.CurrentRankedRating &&
               PreviousRankedRating == data.PreviousRankedRating &&
               RankedRatingDelta == data.RankedRatingDelta &&
               MatchPlacementData.SequenceEqual(data.MatchPlacementData) &&
               NumRankedMatches == data.NumRankedMatches &&
               FansPerRound == data.FansPerRound &&
               FansPerRoundWon == data.FansPerRoundWon &&
               FansGained == data.FansGained &&
               PlayerData.SequenceEqual(data.PlayerData) &&
               UsedTeamRank == data.UsedTeamRank &&
               IsFullTeamInParty == data.IsFullTeamInParty;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(EqualityContract);
        hash.Add(MatchId);
        hash.Add(TeamNumber);
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
}