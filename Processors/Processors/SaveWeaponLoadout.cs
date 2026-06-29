using Packets;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public class SaveWeaponLoadout : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public SaveWeaponLoadout(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnLoadoutServiceRpc.SaveWeaponLoadoutV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        SaveWeaponLoadoutMessage req = Packet.GetPayloadAsMessage<SaveWeaponLoadoutMessage>();
        Model.WeaponLoadout saved = Model.WeaponLoadout.FromPacket(req.WeaponLoadoutData);
        await saved.SyncToDatabase();
        JsonObject resJson = new()
        {
            ["success"] = true,
            ["savedLoadoutId"] = saved.LoadoutId
        };
        return SpectreWebsocketMessage.From(resJson);
    }
}