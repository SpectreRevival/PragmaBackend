using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class MarkMessageAsRead : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public MarkMessageAsRead(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnInboxServiceRpc.MarkMessageAsReadClientV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        var req = Packet.GetPayloadAsMessage<ReadMessageRequest>();
        Model.ClientMessage msg = await Model.ClientMessage.RetrieveFromDatabase(Guid.Parse(req.MessageId));
        msg.ReadTime = DateTimeOffset.UtcNow;
        await msg.SyncToDatabase();
        return SpectreWebsocketMessage.Empty();
    }
}