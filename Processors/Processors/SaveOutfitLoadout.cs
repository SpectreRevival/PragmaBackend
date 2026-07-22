using Google.Protobuf;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public class SaveOutfitLoadout : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    private static readonly JsonParser TolerantParser = new(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

    [SetsRequiredMembers]
    public SaveOutfitLoadout(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnLoadoutServiceRpc.SaveOutfitLoadoutV1Request");
    }

    private static void NormalizeSlot(Packets.OutfitData? slot, Action<Packets.OutfitData> assign)
    {
        slot ??= new Packets.OutfitData();
        if (string.IsNullOrEmpty(slot.ItemInstanceId))
        {
            slot.ItemInstanceId = Guid.Empty.ToString();
        }
        assign(slot);
    }

    private static async Task EnsureItemExists(Packets.OutfitData slot, Guid playerId)
    {
        if (string.IsNullOrEmpty(slot.ItemCatalogId) || !Guid.TryParse(slot.ItemInstanceId, out Guid instanceId) || instanceId == Guid.Empty)
        {
            return;
        }
        if (await Model.CustomizedInstancedItem.RetrieveFromDatabase(instanceId) != null)
        {
            return;
        }
        await new Model.CustomizedInstancedItem(playerId, slot.ItemCatalogId, instanceId, true, []).SyncToDatabase();
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        Packets.OutfitLoadout req = TolerantParser.Parse<Packets.OutfitLoadout>(Packet.RequestPayload.ToJsonString());
        if (string.IsNullOrEmpty(req.PlayerId))
        {
            req.PlayerId = ConnectionHandler.PlayerId.ToString();
        }
        if (string.IsNullOrEmpty(req.LoadoutId))
        {
            req.LoadoutId = Guid.NewGuid().ToString();
        }
        NormalizeSlot(req.HeadData, slot => req.HeadData = slot);
        NormalizeSlot(req.HairData, slot => req.HairData = slot);
        NormalizeSlot(req.FaceStyleData, slot => req.FaceStyleData = slot);
        NormalizeSlot(req.FaceAccessoryData, slot => req.FaceAccessoryData = slot);
        NormalizeSlot(req.OutfitData, slot => req.OutfitData = slot);

        Guid playerId = Guid.Parse(req.PlayerId);
        foreach (Packets.OutfitData slot in new[] { req.HeadData, req.HairData, req.FaceStyleData, req.FaceAccessoryData, req.OutfitData })
        {
            await EnsureItemExists(slot, playerId);
        }

        Model.OutfitLoadout saved = Model.OutfitLoadout.FromPacket(req);
        await saved.SyncToDatabase();
        Log.Information("SaveOutfitLoadout: saved loadout {LoadoutId} for {PlayerId}", saved.LoadoutId, playerId);
        JsonObject resJson = new()
        {
            ["success"] = true,
            ["savedLoadoutId"] = saved.LoadoutId.ToString()
        };
        return SpectreWebsocketMessage.From(resJson);
    }
}