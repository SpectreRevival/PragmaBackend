using Model;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class GetMatchHistoryByPlayerId : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetMatchHistoryByPlayerId(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("GetMatchHistoryByPlayerIdClientV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        var req = Packet.GetPayloadAsMessage<MatchHistoryByPlayerIdRequest>();
        var res = new MultipleMatchHistoryResponse();
        var matches = await MatchHistoryData.GetMatchesForPlayer(Guid.Parse(req.PlayerId), req.Limit,
            DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(req.StartDate)),
            DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(req.EndDate)));
        foreach (var m in matches)
        {
            res.MatchData.Add(m.ToPacket());
        }
        return SpectreWebsocketMessage.From(res);
    }
}