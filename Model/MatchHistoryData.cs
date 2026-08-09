using Npgsql;

namespace Model;

public record class MatchHistoryData : IDatabaseSyncable<MatchHistoryData, Guid>, IInterchangeable<MatchHistoryData, Packets.MatchData>
{
    public required Guid MatchId { get; set; }
    public required DateTimeOffset MatchDate { get; set; }
    public required string QueueName { get; set; }
    public required string QueueGameMode { get; set; }
    public required string QueueGameMap { get; set; }
    public required bool OvertimeEnabled { get; set; }
    public required string Region { get; set; }
    public required bool IsRanked { get; set; }
    public required bool IsAbandonedMatch { get; set; }
    public required Guid[] AbandonedPlayerIds { get; set; }
    // -1 When no team surrendered
    public required int SurrenderedTeam { get; set; }
    public required MatchHistoryTeamData[] TeamData { get; set; }
    public static MatchHistoryData FromPacket(Packets.MatchData inst)
    {
        throw new NotImplementedException();
    }

    public static Task<MatchHistoryData?> RetrieveFromDatabase(Guid key)
    {
        throw new NotImplementedException();
    }

    public NpgsqlBatchCommand CreateBatchSyncCommand()
    {
        throw new NotImplementedException();
    }

    public Guid GetKey()
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }

    public Packets.MatchData ToPacket()
    {
        throw new NotImplementedException();
    }
}