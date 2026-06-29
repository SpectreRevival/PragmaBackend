using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace Model;

// Ohm - Intentionally removed country, subdivision, primary/secondary geographic region and address fields from this struct since we aren't going to store those for privacy reasons.

public record class PlayerMatchmakingData : IDatabaseSyncableDefault<PlayerMatchmakingData, Guid>, IEquatable<PlayerMatchmakingData>, IInterchangeable<PlayerMatchmakingData, Packets.PlayerMatchmakingData>
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
    public required double CasualMMR { get; set; }
    public required double RankedMMR { get; set; }
    public required long SoloRankPoints { get; set; }
    public required long CasualMatchesPlayed { get; set; }
    public required long RankedMatchesPlayed { get; set; }
    public required long CasualMatchesPlayedSeasonal { get; set; }
    public required long RankedMatchesPlayedSeasonal { get; set; }
    public required string[] RankedPlacementMatches { get; set; }
    public required long CurrentSoloRank { get; set; }// Todo replace with enum
    public required long HighestTeamRank { get; set; }// same
    public required long CasualMatchesWon { get; set; }
    public required long RankedMatchesWon { get; set; }
    public required DateTimeOffset PriorityMatchmakingUntil { get; set; }
    public required DateTimeOffset RestrictMatchmakingUntil { get; set; }
    public required string MapHistory { get; set; }

    public static async Task<PlayerMatchmakingData?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_player_matchmaking_data.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new PlayerMatchmakingData(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<double>(1),
            await reader.GetFieldValueAsync<double>(2),
            await reader.GetFieldValueAsync<long>(3),
            await reader.GetFieldValueAsync<long>(4),
            await reader.GetFieldValueAsync<long>(5),
            await reader.GetFieldValueAsync<long>(6),
            await reader.GetFieldValueAsync<long>(7),
            await reader.GetFieldValueAsync<string[]>(8),
            await reader.GetFieldValueAsync<long>(9),
            await reader.GetFieldValueAsync<long>(10),
            await reader.GetFieldValueAsync<long>(11),
            await reader.GetFieldValueAsync<long>(12),
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
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId
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
            && MapHistory == other.MapHistory));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
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

    public static PlayerMatchmakingData CreateDefault(Guid key)
    {
        return new PlayerMatchmakingData(key, 0, 0, 0, 0, 0, 0, 0, [], 0, 0, 0, 0, DateTimeOffset.FromUnixTimeMilliseconds(0), DateTimeOffset.FromUnixTimeMilliseconds(0), "");
    }

    public static PlayerMatchmakingData FromPacket(Packets.PlayerMatchmakingData inst)
    {
        return new PlayerMatchmakingData(Guid.Parse(inst.PlayerId), inst.CasualMmr, inst.RankedMmr, (long)inst.SoloRankPoints, (long)inst.CasualMatchesPlayedCount, (long)inst.RankedMatchesPlayedCount, (long)inst.CasualMatchesPlayedSeasonCount, (long)inst.RankedMatchesPlayedSeasonCount,
            inst.RankedPlacementMatches.ToArray(), (long)inst.CurrentSoloRank, (long)inst.HighestTeamRank, (long)inst.CasualMatchesWonCount, (long)inst.RankedMatchesWonCount, DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.PriorityMatchmakingUntil)), DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.RestrictMatchmakingUntil)), inst.MapHistory);
    }

    public Packets.PlayerMatchmakingData ToPacket()
    {
        Packets.PlayerMatchmakingData packet = new()
        {
            PlayerId = PlayerId.ToString(),
            CasualMmr = CasualMMR,
            RankedMmr = RankedMMR,
            SoloRankPoints = SoloRankPoints,
            CasualMatchesPlayedCount = CasualMatchesPlayed,
            RankedMatchesPlayedCount = RankedMatchesPlayed,
            CasualMatchesPlayedSeasonCount = CasualMatchesPlayedSeasonal,
            RankedMatchesPlayedSeasonCount = RankedMatchesPlayedSeasonal
        };
        foreach (string placementMatch in RankedPlacementMatches)
        {
            packet.RankedPlacementMatches.Add(placementMatch);
        }
        packet.CurrentSoloRank = CurrentSoloRank;
        packet.HighestTeamRank = HighestTeamRank;
        packet.CasualMatchesWonCount = CasualMatchesWon;
        packet.RankedMatchesWonCount = RankedMatchesWon;
        packet.PriorityMatchmakingUntil = PriorityMatchmakingUntil.ToUnixTimeMilliseconds().ToString();
        packet.RestrictMatchmakingUntil = RestrictMatchmakingUntil.ToUnixTimeMilliseconds().ToString();
        packet.MatchmakingFlags = 1.0;
        packet.MapHistory = MapHistory;
        packet.Country = "US";
        packet.Address = "0.0.0.0";
        packet.PrimaryGeographicRegion = "uswest-1";
        packet.SecondaryGeographicRegion = "uscentral-2";
        packet.Subdivision = "CO";
        return packet;
    }
}