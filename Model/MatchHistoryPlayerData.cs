using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public class MatchHistoryPlayerData : IEquatable<MatchHistoryPlayerData?>
{
    [SetsRequiredMembers]
    public MatchHistoryPlayerData(Guid playerId, string nativePlatformId, string savedPlayerName, string selectedBannerCatalogId, string savedSponsorName, string sponsorGameId, bool isAnonymousPlayer, bool hasCrewScoreEarned, int teammateIndex, int numKills, int numAssists, int numDeaths, int totalDamageDone, int currentRankId, int previousRankId, int currentRankedRating, int previousRankedRating, int rankedRatingDelta, int crewScore, Guid crewId, Guid divisionId, int divisionType, string[] matchPlacementData, int numRankedMatches)
    {
        PlayerId = playerId;
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

    public override bool Equals(object? obj)
    {
        return Equals(obj as MatchHistoryPlayerData);
    }

    public bool Equals(MatchHistoryPlayerData? other)
    {
        return other is not null &&
               PlayerId.Equals(other.PlayerId) &&
               NativePlatformId == other.NativePlatformId &&
               SavedPlayerName == other.SavedPlayerName &&
               SelectedBannerCatalogId == other.SelectedBannerCatalogId &&
               SavedSponsorName == other.SavedSponsorName &&
               SponsorGameId == other.SponsorGameId &&
               IsAnonymousPlayer == other.IsAnonymousPlayer &&
               HasCrewScoreEarned == other.HasCrewScoreEarned &&
               TeammateIndex == other.TeammateIndex &&
               NumKills == other.NumKills &&
               NumAssists == other.NumAssists &&
               NumDeaths == other.NumDeaths &&
               TotalDamageDone == other.TotalDamageDone &&
               CurrentRankId == other.CurrentRankId &&
               PreviousRankId == other.PreviousRankId &&
               CurrentRankedRating == other.CurrentRankedRating &&
               PreviousRankedRating == other.PreviousRankedRating &&
               RankedRatingDelta == other.RankedRatingDelta &&
               CrewScore == other.CrewScore &&
               CrewId.Equals(other.CrewId) &&
               DivisionId.Equals(other.DivisionId) &&
               DivisionType == other.DivisionType &&
               EqualityComparer<string[]>.Default.Equals(MatchPlacementData, other.MatchPlacementData) &&
               NumRankedMatches == other.NumRankedMatches;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(PlayerId);
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

    internal NpgsqlBatchCommand CreateSyncCommand(int teamNumber, Guid matchId)
    {
        var cmd = PostgresDatabase.LoadBatchCommandFromFile("save/player_match_history.sql");
        cmd.Parameters.AddWithValue("match_id", matchId);
        cmd.Parameters.AddWithValue("team_number", teamNumber);
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("native_platform_id", NativePlatformId);
        cmd.Parameters.AddWithValue("saved_player_name", SavedPlayerName);
        cmd.Parameters.AddWithValue("selected_banner_catalog_id", SelectedBannerCatalogId);
        cmd.Parameters.AddWithValue("saved_sponsor_name", SavedSponsorName);
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

    public static bool operator ==(MatchHistoryPlayerData? left, MatchHistoryPlayerData? right)
    {
        return EqualityComparer<MatchHistoryPlayerData>.Default.Equals(left, right);
    }

    public static bool operator !=(MatchHistoryPlayerData? left, MatchHistoryPlayerData? right)
    {
        return !(left == right);
    }
}