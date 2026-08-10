using Model;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class GetMatchHistoryByMatchId : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetMatchHistoryByMatchId(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnMatchHistoryServiceRpc.GetMatchHistoryByMatchIdClientV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        var req = Packet.GetPayloadAsMessage<MatchHistoryByMatchIdRequest>();
        var packet = new SingleMatchHistoryResponse();
        packet.MatchData = (await MatchHistoryData.RetrieveFromDatabase(Guid.Parse(req.MatchId))).ToPacket();
        return SpectreWebsocketMessage.From(packet);
    }
}