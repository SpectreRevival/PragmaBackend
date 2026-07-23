using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public class SaveOutfitLoadout : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public SaveOutfitLoadout(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnLoadoutServiceRpc.SaveOutfitLoadoutV1Request");
    }

    // an unset slot arrives with an empty instance id, which Model.OutfitData cannot parse
    private static Packets.OutfitData NormalizeSlot(Packets.OutfitData? slot)
    {
        slot ??= new Packets.OutfitData();
        if (string.IsNullOrEmpty(slot.ItemInstanceId))
        {
            slot.ItemInstanceId = Guid.Empty.ToString();
        }
        return slot;
    }

    // the client saves slots by instance id and never sends a catalog id, which left every
    // stored slot with an empty catalog; resolve it from the owned item instead
    private static async Task<bool> ResolveSlot(Packets.OutfitData slot, Guid playerId)
    {
        if (!Guid.TryParse(slot.ItemInstanceId, out Guid instanceId) || instanceId == Guid.Empty)
        {
            return true;
        }

        Model.CustomizedInstancedItem? item = await Model.CustomizedInstancedItem.RetrieveFromDatabase(instanceId);
        if (item is null || item.OwningPlayerId != playerId)
        {
            Log.Warning("SaveOutfitLoadout: player {PlayerId} tried to equip instance {InstanceId} they do not own", playerId, instanceId);
            return false;
        }

        slot.ItemCatalogId = item.CatalogId;
        return true;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        Packets.OutfitLoadout req = Packet.GetPayloadAsMessage<Packets.OutfitLoadout>();
        if (string.IsNullOrEmpty(req.PlayerId))
        {
            req.PlayerId = ConnectionHandler.PlayerId.ToString();
        }
        if (string.IsNullOrEmpty(req.LoadoutId))
        {
            req.LoadoutId = Guid.NewGuid().ToString();
        }
        req.HeadData = NormalizeSlot(req.HeadData);
        req.HairData = NormalizeSlot(req.HairData);
        req.FaceStyleData = NormalizeSlot(req.FaceStyleData);
        req.FaceAccessoryData = NormalizeSlot(req.FaceAccessoryData);
        req.OutfitData = NormalizeSlot(req.OutfitData);

        Guid playerId = Guid.Parse(req.PlayerId);
        foreach (Packets.OutfitData slot in new[] { req.HeadData, req.HairData, req.FaceStyleData, req.FaceAccessoryData, req.OutfitData })
        {
            if (!await ResolveSlot(slot, playerId))
            {
                return SpectreWebsocketMessage.From("{\"success\":false}");
            }
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
