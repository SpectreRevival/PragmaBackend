using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class HeartbeatPacketProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public HeartbeatPacketProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("PlayerSessionRpc.HeartbeatV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        return SpectreWebsocketMessage.From("{}");
    }
}