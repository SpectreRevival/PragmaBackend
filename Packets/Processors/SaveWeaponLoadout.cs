using Model;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Packets.Processors;

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

    private static Model.WeaponData ConvertWeaponData(WeaponData packet)
    {
        WeaponAttachment? attachment = null;
        if (packet.AttachmentItemInstanceId != "")
        {
            attachment = new WeaponAttachment(Guid.Parse(packet.AttachmentItemInstanceId), packet.AttachmentItemCatalogId);
        }
        List<Model.ActiveAlterationData> altData = new();
        foreach (var alteration in packet.AlterationData)
        {
            altData.Add(new Model.ActiveAlterationData(
                alteration.ChannelId,
                alteration.AlterationId
            ));
        }
        return new Model.WeaponData(Guid.Parse(packet.ItemInstanceId), altData.ToArray(), attachment, packet.ItemCatalogId);
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        SaveWeaponLoadoutMessage req = Packet.GetPayloadAsMessage<SaveWeaponLoadoutMessage>();
        Model.WeaponLoadout saved = new(
            Guid.Parse(req.WeaponLoadoutData.PlayerId),
            Guid.Parse(req.WeaponLoadoutData.LoadoutId),
            ConvertWeaponData(req.WeaponLoadoutData.SemiautoPistolData),
            ConvertWeaponData(req.WeaponLoadoutData.SuppressedPistolData),
            ConvertWeaponData(req.WeaponLoadoutData.AutoPistolData),
            ConvertWeaponData(req.WeaponLoadoutData.HighcalPistolData),
            ConvertWeaponData(req.WeaponLoadoutData.HeavyShotgunData),
            ConvertWeaponData(req.WeaponLoadoutData.AutoShotgunData),
            ConvertWeaponData(req.WeaponLoadoutData.TacticalSmgData),
            ConvertWeaponData(req.WeaponLoadoutData.RapidfireSmgData),
            ConvertWeaponData(req.WeaponLoadoutData.SuppressedSmgData),
            ConvertWeaponData(req.WeaponLoadoutData.StandardArData),
            ConvertWeaponData(req.WeaponLoadoutData.SemiautoArData),
            ConvertWeaponData(req.WeaponLoadoutData.BurstArData),
            ConvertWeaponData(req.WeaponLoadoutData.TacticalArData),
            ConvertWeaponData(req.WeaponLoadoutData.SuppressedArData),
            ConvertWeaponData(req.WeaponLoadoutData.HeavyArData),
            ConvertWeaponData(req.WeaponLoadoutData.HighcalMgData),
            ConvertWeaponData(req.WeaponLoadoutData.RapidfireMgData),
            ConvertWeaponData(req.WeaponLoadoutData.SemiautoSniperData),
            ConvertWeaponData(req.WeaponLoadoutData.BoltactionSniperData),
            ConvertWeaponData(req.WeaponLoadoutData.MeleeData)
        );
        await saved.SyncToDatabase();
        JsonObject resJson = new();
        resJson["success"] = true;
        resJson["savedLoadoutId"] = saved.LoadoutId;
        return SpectreWebsocketMessage.From(resJson);
    }
}