using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

// crews are not implemented; and for some reason the client wont do anything if this rpc is unanswered

public class GetCrewByPlayerIdProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetCrewByPlayerIdProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnCrewServiceRpc.GetCrewByPlayerIdV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        return SpectreWebsocketMessage.From("{}");
    }
}

public class GetCrewEndOfMatchDataProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetCrewEndOfMatchDataProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnCrewServiceRpc.GetCrewEndOfMatchDataV2Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        return SpectreWebsocketMessage.From("{}");
    }
}