using Google.Protobuf;
using System.Diagnostics.CodeAnalysis;

namespace Packets;

public abstract class WebsocketPacketProcessor
{
    private static readonly Dictionary<SpectreRpcType, WebsocketPacketProcessor> processors = new();
    public required SpectreRpcType RpcType { get; init; }

    [SetsRequiredMembers]
    protected WebsocketPacketProcessor(SpectreRpcType rpcType)
    {
        RpcType = rpcType;
    }
    public abstract Task<IMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler);

    public static WebsocketPacketProcessor? GetProcessorForRequestType(SpectreRpcType rpcType)
    {
        if(processors.TryGetValue(rpcType, out WebsocketPacketProcessor? processor))
        {
            return processor;
        }
        return null;
    }
}