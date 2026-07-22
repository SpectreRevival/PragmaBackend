using Packets;
using Serilog;
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

    // charms arrive as an attachment instance id with no catalog id; resolve it from the owned
    // item so the stored loadout carries both halves
    private static async Task BackfillAttachmentCatalogIds(Packets.WeaponLoadout loadout, Guid playerId)
    {
        Packets.WeaponData[] slots =
        [
            loadout.SemiautoPistolData, loadout.SuppressedPistolData, loadout.AutoPistolData, loadout.HighcalPistolData,
            loadout.HeavyShotgunData, loadout.AutoShotgunData, loadout.TacticalSmgData, loadout.RapidfireSmgData,
            loadout.SuppressedSmgData, loadout.StandardArData, loadout.SemiautoArData, loadout.BurstArData,
            loadout.TacticalArData, loadout.SuppressedArData, loadout.HeavyArData, loadout.HighcalMgData,
            loadout.RapidfireMgData, loadout.SemiautoSniperData, loadout.BoltactionSniperData, loadout.MeleeData
        ];

        foreach (Packets.WeaponData slot in slots)
        {
            if (slot is null || !string.IsNullOrEmpty(slot.AttachmentItemCatalogId))
            {
                continue;
            }
            if (!Guid.TryParse(slot.AttachmentItemInstanceId, out Guid attachmentId) || attachmentId == Guid.Empty)
            {
                continue;
            }

            Model.CustomizedInstancedItem? charm = await Model.CustomizedInstancedItem.RetrieveFromDatabase(attachmentId);
            if (charm is null || charm.OwningPlayerId != playerId)
            {
                Log.Warning("SaveWeaponLoadout: player {PlayerId} tried to equip charm instance {InstanceId} they do not own, dropping it",
                    playerId, attachmentId);
                slot.AttachmentItemInstanceId = "";
                continue;
            }
            slot.AttachmentItemCatalogId = charm.CatalogId;
        }
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        SaveWeaponLoadoutMessage req = Packet.GetPayloadAsMessage<SaveWeaponLoadoutMessage>();
        await BackfillAttachmentCatalogIds(req.WeaponLoadoutData, ConnectionHandler.PlayerId);
        Model.WeaponLoadout saved = Model.WeaponLoadout.FromPacket(req.WeaponLoadoutData);
        await saved.SyncToDatabase();
        Log.Information("SaveWeaponLoadout: saved loadout {LoadoutId} for {PlayerId}", saved.LoadoutId, saved.PlayerId);
        JsonObject resJson = new()
        {
            ["success"] = true,
            ["savedLoadoutId"] = saved.LoadoutId
        };
        return SpectreWebsocketMessage.From(resJson);
    }
}