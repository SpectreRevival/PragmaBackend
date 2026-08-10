using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class MatchHistoryData : IDatabaseSyncable<MatchHistoryData, Guid>, IInterchangeable<MatchHistoryData, Packets.MatchData>
{
    private static readonly JsonSerializerOptions jsonSquaredRenderer = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [SetsRequiredMembers]
    public MatchHistoryData(Guid matchId, DateTimeOffset matchDate, string queueName, string queueGameMode, string queueGameMap, bool overtimeEnabled, string region, bool isRanked, bool isAbandonedMatch, Guid[] abandonedPlayerIds, int surrenderedTeam, MatchHistoryTeamData[] teamData)
    {
        MatchId = matchId;
        MatchDate = matchDate;
        QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        QueueGameMode = queueGameMode ?? throw new ArgumentNullException(nameof(queueGameMode));
        QueueGameMap = queueGameMap ?? throw new ArgumentNullException(nameof(queueGameMap));
        OvertimeEnabled = overtimeEnabled;
        Region = region ?? throw new ArgumentNullException(nameof(region));
        IsRanked = isRanked;
        IsAbandonedMatch = isAbandonedMatch;
        AbandonedPlayerIds = abandonedPlayerIds ?? throw new ArgumentNullException(nameof(abandonedPlayerIds));
        SurrenderedTeam = surrenderedTeam;
        TeamData = teamData ?? throw new ArgumentNullException(nameof(teamData));
    }

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
        var jobj = JsonObject.Parse(inst.MatchData_);
        jobj["teamData"].AsArray().ForEach(teamData => teamData["playerData"].AsArray().ForEach(playerData =>
        {
            var sponsorGameId = playerData["selectedSponsor"]["tagName"].GetValue<string>();
            playerData["selectedSponsor"] = null;
            playerData["sponsorGameId"] = sponsorGameId;
        }));
        return JsonSerializer.Deserialize<MatchHistoryData>(jobj.ToJsonString(), jsonSquaredRenderer) ?? throw new InvalidOperationException("Failed to deserialize MatchHistoryData from packet.");
    }
    internal static MatchHistoryData GetFromReader(NpgsqlDataReader reader)
    {
        return new MatchHistoryData(
            matchId: reader.GetGuid(0),
            matchDate: reader.GetFieldValue<DateTimeOffset>(1),
            queueName: reader.GetString(2),
            queueGameMode: reader.GetString(3),
            queueGameMap: reader.GetString(4),
            overtimeEnabled: reader.GetBoolean(5),
            region: reader.GetString(6),
            isRanked: reader.GetBoolean(7),
            isAbandonedMatch: reader.GetBoolean(8),
            abandonedPlayerIds: reader.GetFieldValue<Guid[]>(9),
            surrenderedTeam: reader.GetInt32(10),
            teamData: Array.Empty<MatchHistoryTeamData>()
        );
    }

    public static async Task<MatchHistoryData?> RetrieveFromDatabase(Guid matchId)
    {
        var cmd = PostgresDatabase.LoadCommandFromFile("query/match_history.sql");
        cmd.Parameters.AddWithValue("match_id", matchId);
        using var reader = await cmd.ExecuteReaderAsync();
        if(!await reader.ReadAsync())
        {
            return null;
        }
        var ret = GetFromReader(reader);
        ret.TeamData = await MatchHistoryTeamData.RetrieveFromDatabase(matchId);
        return ret;
    }

    public Guid GetKey()
    {
        return MatchId;
    }

    private NpgsqlBatchCommand CreateBatchSyncBasicCommand()
    {
        var cmd = PostgresDatabase.LoadBatchCommandFromFile("save/match_history.sql");
        cmd.Parameters.AddWithValue("match_id", MatchId);
        cmd.Parameters.AddWithValue("match_date", MatchDate);
        cmd.Parameters.AddWithValue("queue_name", QueueName);
        cmd.Parameters.AddWithValue("queue_game_mode", QueueGameMode);
        cmd.Parameters.AddWithValue("queue_game_map", QueueGameMap);
        cmd.Parameters.AddWithValue("overtime_enabled", OvertimeEnabled);
        cmd.Parameters.AddWithValue("region", Region);
        cmd.Parameters.AddWithValue("is_ranked", IsRanked);
        cmd.Parameters.AddWithValue("is_abandoned_match", IsAbandonedMatch);
        cmd.Parameters.AddWithValue("abandoned_player_ids", AbandonedPlayerIds);
        cmd.Parameters.AddWithValue("surrendered_team", SurrenderedTeam);
        return cmd;
    }

    public async Task SyncToDatabase()
    {
        var batch = PostgresDatabase.CreateBatch();
        for(int i = 0; i < TeamData.Length; i++)
        {
            TeamData[i].AddSyncToBatch(batch, MatchId, i);
        }
        batch.BatchCommands.Add(CreateBatchSyncBasicCommand());
        await batch.ExecuteNonQueryAsync();
    }

    public Packets.MatchData ToPacket()
    {
        var jstr = JsonSerializer.Serialize(this, jsonSquaredRenderer);
        var jobj = JsonObject.Parse(jstr);
        jobj["teamData"].AsArray().ForEach(item => item["playerData"].AsArray().ForEach(playerData =>
        {
            var sponsorGameId = playerData["sponsorGameId"].GetValue<string>();
            playerData["sponsorGameId"] = null;
            playerData["selectedSponsor"] = JsonNode.Parse("{\"tagName\":\"" + sponsorGameId + "\"}");
        }));
        var packet = new Packets.MatchData();
        packet.MatchId = MatchId.ToString();
        packet.MatchDate = MatchDate.ToUnixTimeMilliseconds().ToString();
        packet.MatchData_ = jobj.ToJsonString();
        return packet;
    }

    public IEnumerable<NpgsqlBatchCommand> CreateBatchSyncCommand()
    {
        var commands = new List<NpgsqlBatchCommand>() { CreateBatchSyncBasicCommand() };
        TeamData.Select((team, i) => team.GetBatchCommands()).ForEach(cmdarr => commands.AddRange(cmdarr));
        return commands;
    }

    public virtual bool Equals(MatchHistoryData? data)
    {
        return data is not null &&
               EqualityComparer<Type>.Default.Equals(EqualityContract, data.EqualityContract) &&
               MatchId.Equals(data.MatchId) &&
               MatchDate.Equals(data.MatchDate) &&
               QueueName == data.QueueName &&
               QueueGameMode == data.QueueGameMode &&
               QueueGameMap == data.QueueGameMap &&
               OvertimeEnabled == data.OvertimeEnabled &&
               Region == data.Region &&
               IsRanked == data.IsRanked &&
               IsAbandonedMatch == data.IsAbandonedMatch &&
               EqualityComparer<Guid[]>.Default.Equals(AbandonedPlayerIds, data.AbandonedPlayerIds) &&
               SurrenderedTeam == data.SurrenderedTeam &&
               EqualityComparer<MatchHistoryTeamData[]>.Default.Equals(TeamData, data.TeamData);
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(EqualityContract);
        hash.Add(MatchId);
        hash.Add(MatchDate);
        hash.Add(QueueName);
        hash.Add(QueueGameMode);
        hash.Add(QueueGameMap);
        hash.Add(OvertimeEnabled);
        hash.Add(Region);
        hash.Add(IsRanked);
        hash.Add(IsAbandonedMatch);
        hash.Add(AbandonedPlayerIds);
        hash.Add(SurrenderedTeam);
        hash.Add(TeamData);
        return hash.ToHashCode();
    }
}