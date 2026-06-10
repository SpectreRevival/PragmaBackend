using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

public class UpdatePresenceForPlayerProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public UpdatePresenceForPlayerProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnPlayerPresenceServiceRpc.UpdatePresenceForPlayerV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        UpdatePlayerPresenceRequest req = Packet.GetPayloadAsMessage<UpdatePlayerPresenceRequest>();
        Model.PlayerPresence newPresence = new(
            ConnectionHandler.PlayerId,
            req.PlayerPresence.IsOnline == true ? Model.PlayerBasicPresence.Online : Model.PlayerBasicPresence.Offline,
            DateTimeOffset.UtcNow,
            req.PlayerPresence.MainPresenceId,
            req.PlayerPresence.PresenceContext
        );
        await newPresence.SyncToDatabase();
        return SpectreWebsocketMessage.From("{\"success\":true}");
    }
}