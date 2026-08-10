using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class MatchHistoryPlayerData
{
    [SetsRequiredMembers]
    public MatchHistoryPlayerData(Guid playerId, Guid matchId, int teamNumber, string nativePlatformId, string savedPlayerName, string selectedBannerCatalogId, string savedSponsorName, string sponsorGameId, bool isAnonymousPlayer, bool hasCrewScoreEarned, int teammateIndex, int numKills, int numAssists, int numDeaths, int totalDamageDone, int currentRankId, int previousRankId, int currentRankedRating, int previousRankedRating, int rankedRatingDelta, int crewScore, Guid crewId, Guid divisionId, int divisionType, string[] matchPlacementData, int numRankedMatches)
    {
        PlayerId = playerId;
        MatchId = matchId;
        TeamNumber = teamNumber;
        NativePlatformId = nativePlatformId ?? throw new ArgumentNullException(nameof(nativePlatformId));
        SavedPlayerName = savedPlayerName ?? throw new ArgumentNullException(nameof(savedPlayerName));
        SelectedBannerCatalogId = selectedBannerCatalogId ?? throw new ArgumentNullException(nameof(selectedBannerCatalogId));
        SavedSponsorName = savedSponsorName ?? throw new ArgumentNullException(nameof(savedSponsorName));
        SponsorGameId = sponsorGameId ?? throw new ArgumentNullException(nameof(sponsorGameId));
        IsAnonymousPlayer = isAnonymousPlayer;
        HasCrewScoreEarned = hasCrewScoreEarned;
        TeammateIndex = teammateIndex;
        NumKills = numKills;
        NumAssists = numAssists;
        NumDeaths = numDeaths;
        TotalDamageDone = totalDamageDone;
        CurrentRankId = currentRankId;
        PreviousRankId = previousRankId;
        CurrentRankedRating = currentRankedRating;
        PreviousRankedRating = previousRankedRating;
        RankedRatingDelta = rankedRatingDelta;
        CrewScore = crewScore;
        CrewId = crewId;
        DivisionId = divisionId;
        DivisionType = divisionType;
        MatchPlacementData = matchPlacementData ?? throw new ArgumentNullException(nameof(matchPlacementData));
        NumRankedMatches = numRankedMatches;
    }

    public required Guid PlayerId { get; set; }
    public required Guid MatchId { get; set; }
    public required int TeamNumber { get; set; }
    public required string NativePlatformId { get; set; }
    public required string SavedPlayerName { get; set; }
    public required string SelectedBannerCatalogId { get; set; }
    /** The user-known name of the sponsor, eg. Bloom */
    public required string SavedSponsorName { get; set; }
    /** The internal ID of the sponsor, eg. Sponsor.Guardian */
    public required string SponsorGameId { get; set; }
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

    public NpgsqlBatchCommand CreateSyncCommand()
    {
        var cmd = PostgresDatabase.LoadBatchCommandFromFile("save/player_match_history.sql");
        cmd.Parameters.AddWithValue("match_id", MatchId);
        cmd.Parameters.AddWithValue("team_number", TeamNumber);
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("native_platform_id", NativePlatformId);
        cmd.Parameters.AddWithValue("saved_player_name", SavedPlayerName);
        cmd.Parameters.AddWithValue("selected_banner_catalog_id", SelectedBannerCatalogId);
        cmd.Parameters.AddWithValue("saved_sponsor_name", SavedSponsorName);
        cmd.Parameters.AddWithValue("sponsor_game_id", SponsorGameId);
        cmd.Parameters.AddWithValue("is_anonymous_player", IsAnonymousPlayer);
        cmd.Parameters.AddWithValue("has_crew_score_earned", HasCrewScoreEarned);
        cmd.Parameters.AddWithValue("teammate_index", TeammateIndex);
        cmd.Parameters.AddWithValue("num_kills", NumKills);
        cmd.Parameters.AddWithValue("num_assists", NumAssists);
        cmd.Parameters.AddWithValue("num_deaths", NumDeaths);
        cmd.Parameters.AddWithValue("total_damage", TotalDamageDone);
        cmd.Parameters.AddWithValue("current_rank_id", CurrentRankId);
        cmd.Parameters.AddWithValue("previous_rank_id", PreviousRankId);
        cmd.Parameters.AddWithValue("current_ranked_rating", CurrentRankedRating);
        cmd.Parameters.AddWithValue("previous_ranked_rating", PreviousRankedRating);
        cmd.Parameters.AddWithValue("ranked_rating_delta", RankedRatingDelta);
        cmd.Parameters.AddWithValue("crew_id", CrewId);
        cmd.Parameters.AddWithValue("division_id", DivisionId);
        cmd.Parameters.AddWithValue("division_type", DivisionType);
        cmd.Parameters.AddWithValue("match_placement_data", MatchPlacementData);
        cmd.Parameters.AddWithValue("num_ranked_matches", NumRankedMatches);
        return cmd;
    }

    public static async Task<MatchHistoryPlayerData?> RetrieveFromDatabase(Guid matchId, Guid playerId)
    {
        var cmd = PostgresDatabase.LoadCommandFromFile("query/player_match_history.sql");
        cmd.Parameters.AddWithValue("match_id", matchId);
        cmd.Parameters.AddWithValue("player_id", playerId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return GetFromReader(reader);
    }
    internal static MatchHistoryPlayerData GetFromReader(NpgsqlDataReader reader)
    {
        return new MatchHistoryPlayerData(
            playerId: reader.GetGuid(0),
            matchId: reader.GetGuid(1),
            teamNumber: reader.GetInt32(2),
            nativePlatformId: reader.GetString(3),
            savedPlayerName: reader.GetString(4),
            selectedBannerCatalogId: reader.GetString(5),
            savedSponsorName: reader.GetString(6),
            sponsorGameId: reader.GetString(7),
            isAnonymousPlayer: reader.GetBoolean(8),
            hasCrewScoreEarned: reader.GetBoolean(9),
            teammateIndex: reader.GetInt32(10),
            numKills: reader.GetInt32(11),
            numAssists: reader.GetInt32(12),
            numDeaths: reader.GetInt32(13),
            totalDamageDone: reader.GetInt32(14),
            currentRankId: reader.GetInt32(15),
            previousRankId: reader.GetInt32(16),
            currentRankedRating: reader.GetInt32(17),
            previousRankedRating: reader.GetInt32(18),
            rankedRatingDelta: reader.GetInt32(19),
            crewScore: reader.GetInt32(20),
            crewId: reader.GetGuid(21),
            divisionId: reader.GetGuid(22),
            divisionType: reader.GetInt32(23),
            matchPlacementData: reader.GetFieldValue<string[]>(24),
            numRankedMatches: reader.GetInt32(25)
        );
    }

    public static async Task<MatchHistoryPlayerData[]> RetrieveFromDatabase(Guid matchId, int teamNumber)
    {
        var cmd = PostgresDatabase.LoadCommandFromFile("query/player_match_history_all_team.sql");
        cmd.Parameters.AddWithValue("match_id", matchId);
        cmd.Parameters.AddWithValue("team_number", teamNumber);
        using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<MatchHistoryPlayerData>();
        while (await reader.ReadAsync())
        {
            results.Add(GetFromReader(reader));
        }
        return results.ToArray();
    }

    public static async Task<MatchHistoryPlayerData[]> RetrieveFromDatabase(Guid matchId)
    {
        var cmd = PostgresDatabase.LoadCommandFromFile("query/player_match_history_all_match.sql");
        cmd.Parameters.AddWithValue("match_id", matchId);
        using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<MatchHistoryPlayerData>();
        while (await reader.ReadAsync())
        {
            results.Add(GetFromReader(reader));
        }
        return results.ToArray();
    }

    public virtual bool Equals(MatchHistoryPlayerData? data)
    {
        return data is not null &&
               EqualityComparer<Type>.Default.Equals(EqualityContract, data.EqualityContract) &&
               PlayerId.Equals(data.PlayerId) &&
               MatchId.Equals(data.MatchId) &&
               TeamNumber == data.TeamNumber &&
               NativePlatformId == data.NativePlatformId &&
               SavedPlayerName == data.SavedPlayerName &&
               SelectedBannerCatalogId == data.SelectedBannerCatalogId &&
               SavedSponsorName == data.SavedSponsorName &&
               SponsorGameId == data.SponsorGameId &&
               IsAnonymousPlayer == data.IsAnonymousPlayer &&
               HasCrewScoreEarned == data.HasCrewScoreEarned &&
               TeammateIndex == data.TeammateIndex &&
               NumKills == data.NumKills &&
               NumAssists == data.NumAssists &&
               NumDeaths == data.NumDeaths &&
               TotalDamageDone == data.TotalDamageDone &&
               CurrentRankId == data.CurrentRankId &&
               PreviousRankId == data.PreviousRankId &&
               CurrentRankedRating == data.CurrentRankedRating &&
               PreviousRankedRating == data.PreviousRankedRating &&
               RankedRatingDelta == data.RankedRatingDelta &&
               CrewScore == data.CrewScore &&
               CrewId.Equals(data.CrewId) &&
               DivisionId.Equals(data.DivisionId) &&
               DivisionType == data.DivisionType &&
               MatchPlacementData.SequenceEqual(data.MatchPlacementData) &&
               NumRankedMatches == data.NumRankedMatches;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(EqualityContract);
        hash.Add(PlayerId);
        hash.Add(MatchId);
        hash.Add(TeamNumber);
        hash.Add(NativePlatformId);
        hash.Add(SavedPlayerName);
        hash.Add(SelectedBannerCatalogId);
        hash.Add(SavedSponsorName);
        hash.Add(SponsorGameId);
        hash.Add(IsAnonymousPlayer);
        hash.Add(HasCrewScoreEarned);
        hash.Add(TeammateIndex);
        hash.Add(NumKills);
        hash.Add(NumAssists);
        hash.Add(NumDeaths);
        hash.Add(TotalDamageDone);
        hash.Add(CurrentRankId);
        hash.Add(PreviousRankId);
        hash.Add(CurrentRankedRating);
        hash.Add(PreviousRankedRating);
        hash.Add(RankedRatingDelta);
        hash.Add(CrewScore);
        hash.Add(CrewId);
        hash.Add(DivisionId);
        hash.Add(DivisionType);
        hash.Add(MatchPlacementData);
        hash.Add(NumRankedMatches);
        return hash.ToHashCode();
    }
}