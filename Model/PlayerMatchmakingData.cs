using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

// Ohm - Intentionally removed country, subdivision, primary/secondary geographic region and address fields from this struct since we aren't going to store those for privacy reasons.

public record class PlayerMatchmakingData : IDatabaseSyncable<PlayerMatchmakingData, Guid>, IEquatable<PlayerMatchmakingData>
{
    [SetsRequiredMembers]
    public PlayerMatchmakingData(Guid playerId, double casualMMR, double rankedMMR, long soloRankPoints, long casualMatchesPlayed, long rankedMatchesPlayed, long casualMatchesPlayedSeasonal, long rankedMatchesPlayedSeasonal, string[] rankedPlacementMatches, long currentSoloRank, long highestTeamRank, long casualMatchesWon, long rankedMatchesWon, DateTimeOffset priorityMatchmakingUntil, DateTimeOffset restrictMatchmakingUntil, string mapHistory)
    {
        PlayerId = playerId;
        CasualMMR = casualMMR;
        RankedMMR = rankedMMR;
        SoloRankPoints = soloRankPoints;
        CasualMatchesPlayed = casualMatchesPlayed;
        RankedMatchesPlayed = rankedMatchesPlayed;
        CasualMatchesPlayedSeasonal = casualMatchesPlayedSeasonal;
        RankedMatchesPlayedSeasonal = rankedMatchesPlayedSeasonal;
        RankedPlacementMatches = rankedPlacementMatches ?? throw new ArgumentNullException(nameof(rankedPlacementMatches));
        CurrentSoloRank = currentSoloRank;
        HighestTeamRank = highestTeamRank;
        CasualMatchesWon = casualMatchesWon;
        RankedMatchesWon = rankedMatchesWon;
        PriorityMatchmakingUntil = priorityMatchmakingUntil;
        RestrictMatchmakingUntil = restrictMatchmakingUntil;
        MapHistory = mapHistory ?? throw new ArgumentNullException(nameof(mapHistory));
    }

    public required Guid PlayerId { get; set; }
    public required double CasualMMR { get; set; } = 0.0d;
    public required double RankedMMR { get; set; } = 0.0d;
    public required Int64 SoloRankPoints { get; set; } = 0;
    public required Int64 CasualMatchesPlayed { get; set; } = 0;
    public required Int64 RankedMatchesPlayed { get; set; } = 0;
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

    public static async Task<PlayerMatchmakingData?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_player_matchmaking_data.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new PlayerMatchmakingData(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<double>(1),
            await reader.GetFieldValueAsync<double>(2),
            await reader.GetFieldValueAsync<Int64>(3),
            await reader.GetFieldValueAsync<Int64>(4),
            await reader.GetFieldValueAsync<Int64>(5),
            await reader.GetFieldValueAsync<Int64>(6),
            await reader.GetFieldValueAsync<Int64>(7),
            await reader.GetFieldValueAsync<string[]>(8),
            await reader.GetFieldValueAsync<Int64>(9),
            await reader.GetFieldValueAsync<Int64>(10),
            await reader.GetFieldValueAsync<Int64>(11),
            await reader.GetFieldValueAsync<Int64>(12),
            await reader.GetFieldValueAsync<DateTimeOffset>(13),
            await reader.GetFieldValueAsync<DateTimeOffset>(14),
            await reader.GetFieldValueAsync<string>(15)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_player_matchmaking_data.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("casual_mmr", CasualMMR);
        cmd.Parameters.AddWithValue("ranked_mmr", RankedMMR);
        cmd.Parameters.AddWithValue("solo_rank_points", SoloRankPoints);
        cmd.Parameters.AddWithValue("casual_matches_played", CasualMatchesPlayed);
        cmd.Parameters.AddWithValue("ranked_matches_played", RankedMatchesPlayed);
        cmd.Parameters.AddWithValue("casual_matches_played_seasonal", CasualMatchesPlayedSeasonal);
        cmd.Parameters.AddWithValue("ranked_matches_played_seasonal", RankedMatchesPlayedSeasonal);
        cmd.Parameters.AddWithValue("ranked_placement_matches", RankedPlacementMatches);
        cmd.Parameters.AddWithValue("current_solo_rank", CurrentSoloRank);
        cmd.Parameters.AddWithValue("highest_team_rank", HighestTeamRank);
        cmd.Parameters.AddWithValue("casual_matches_won", CasualMatchesWon);
        cmd.Parameters.AddWithValue("ranked_matches_won", RankedMatchesWon);
        cmd.Parameters.AddWithValue("priority_matchmaking_until", PriorityMatchmakingUntil);
        cmd.Parameters.AddWithValue("restrict_matchmaking_until", RestrictMatchmakingUntil);
        cmd.Parameters.AddWithValue("map_history", MapHistory);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(PlayerMatchmakingData? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
            && CasualMMR == other.CasualMMR
            && RankedMMR == other.RankedMMR
            && SoloRankPoints == other.SoloRankPoints
            && CasualMatchesPlayed == other.CasualMatchesPlayed
            && RankedMatchesPlayed == other.RankedMatchesPlayed
            && CasualMatchesPlayedSeasonal == other.CasualMatchesPlayedSeasonal
            && RankedMatchesPlayedSeasonal == other.RankedMatchesPlayedSeasonal
            && RankedPlacementMatches.SequenceEqual(other.RankedPlacementMatches)
            && CurrentSoloRank == other.CurrentSoloRank
            && HighestTeamRank == other.HighestTeamRank
            && CasualMatchesWon == other.CasualMatchesWon
            && RankedMatchesWon == other.RankedMatchesWon
            && PriorityMatchmakingUntil.ToUnixTimeMilliseconds() == other.PriorityMatchmakingUntil.ToUnixTimeMilliseconds()
            && RestrictMatchmakingUntil.ToUnixTimeMilliseconds() == other.RestrictMatchmakingUntil.ToUnixTimeMilliseconds()
            && MapHistory == other.MapHistory;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(PlayerId);
        hash.Add(CasualMMR);
        hash.Add(RankedMMR);
        hash.Add(SoloRankPoints);
        hash.Add(CasualMatchesPlayed);
        hash.Add(RankedMatchesPlayed);
        hash.Add(CasualMatchesPlayedSeasonal);
        hash.Add(RankedMatchesPlayedSeasonal);
        hash.Add(RankedPlacementMatches);
        hash.Add(CurrentSoloRank);
        hash.Add(HighestTeamRank);
        hash.Add(CasualMatchesWon);
        hash.Add(RankedMatchesWon);
        hash.Add(PriorityMatchmakingUntil.ToUnixTimeMilliseconds());
        hash.Add(RestrictMatchmakingUntil.ToUnixTimeMilliseconds());
        hash.Add(MapHistory);
        return hash.ToHashCode();
    }
}