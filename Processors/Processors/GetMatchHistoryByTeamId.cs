using Model;
using Model.Persistence;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class GetMatchHistoryByTeamId : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetMatchHistoryByTeamId(SpectreRpcType rpcType) : base(rpcType)
    {
    }
    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("GetMatchHistoryByTeamIdClientV1Request");
    }
    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        var res = new MultipleMatchHistoryResponse();
        try
        {
            var req = Packet.GetPayloadAsMessage<MatchHistoryByTeamIdRequest>();
            res.MatchData.AddRange((await MatchHistoryData.GetMatchesForTeam(Guid.Parse(req.TeamId), req.Limit,
                DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(req.StartDate)),
                DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(req.EndDate))))
                .Select(mData => mData.ToPacket()));
        } catch (Exception ex) {
            var req = Packet.GetPayloadAsMessage<MatchHistoryByTeamPlayerIdsRequest>();
            var cmd = PostgresDatabase.LoadCommandFromFile("match_history_teamid_from_players.sql");
            cmd.Parameters.AddWithValue("player_ids", req.TeamPlayerIds);
            using var reader = await cmd.ExecuteReaderAsync();
            if(!await reader.ReadAsync())
            {
                throw new InvalidDataException("team id not found for request with player ids of team");
            }
            var teamId = reader.GetGuid(0);
            res.MatchData.AddRange((await MatchHistoryData.GetMatchesForTeam(teamId, req.Limit,
                DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(req.StartDate)),
                DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(req.EndDate))))
                .Select(mData => mData.ToPacket()));
        }
        return SpectreWebsocketMessage.From(res);
    }
}