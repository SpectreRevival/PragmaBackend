using Model;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class FetchDisplayName : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public FetchDisplayName(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("AccountRpc.GetDisplayNameForPragmaPlayerIdV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        FetchDisplayNameReq req = Packet.GetPayloadAsMessage<FetchDisplayNameReq>();
        ProfileData playerProfileData = await Model.ProfileData.RetrieveFromDatabase(Guid.Parse(req.PragmaPlayerId));
        return SpectreWebsocketMessage.From(playerProfileData.DisplayName.ToPacket());
    }
}