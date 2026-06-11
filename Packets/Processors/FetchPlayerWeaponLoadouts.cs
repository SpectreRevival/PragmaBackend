using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

public class FetchPlayerWeaponLoadouts : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public FetchPlayerWeaponLoadouts(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnLoadoutServiceRpc.FetchPlayerWeaponLoadoutsV1Request");
    }

    private static WeaponData WeaponDataFromModelData(Model.WeaponData weaponData)
    {
        WeaponData data = new();
        data.ItemInstanceId = weaponData.ItemInstanceId.ToString();
        data.ItemCatalogId = weaponData.ItemCatalogId;
        if (weaponData.Attachment != null)
        {
            data.AttachmentItemInstanceId = weaponData.Attachment.AttachmentItemInstanceId.ToString();
            data.AttachmentItemCatalogId = weaponData.Attachment.AttachmentItemCatalogId;
        }
        foreach (Model.ActiveAlterationData altData in weaponData.AlterationData)
        {
            ActiveAlterationData packetAltData = new();
            packetAltData.AlterationId = altData.AlterationId;
            packetAltData.ChannelId = altData.ChannelId;
            data.AlterationData.Add(packetAltData);
        }
        return data;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        FetchLoadoutsRequest req = Packet.GetPayloadAsMessage<FetchLoadoutsRequest>();
        NpgsqlCommand cmd = PostgresDatabase.Get().GetRaw().CreateCommand("SELECT loadout_id FROM weapon_loadouts WHERE player_id = @player_id");
        cmd.Parameters.AddWithValue("player_id", Guid.Parse(req.PlayerId));
        await using var reader = await cmd.ExecuteReaderAsync();
        WeaponLoadouts responseData = new();
        while(await reader.ReadAsync())
        {
            Guid loadoutId = reader.GetGuid(0);
            Model.WeaponLoadout curLoadout = await Model.WeaponLoadout.RetrieveFromDatabase(loadoutId);
            WeaponLoadout packetLoadout = new();
            packetLoadout.SemiautoPistolData = WeaponDataFromModelData(curLoadout.SemiAutoPistol);
            packetLoadout.SuppressedPistolData = WeaponDataFromModelData(curLoadout.SuppressedPistol);
            packetLoadout.AutoPistolData = WeaponDataFromModelData(curLoadout.AutoPistol);
            packetLoadout.HighcalPistolData = WeaponDataFromModelData(curLoadout.HighcalPistol);
            packetLoadout.HeavyShotgunData = WeaponDataFromModelData(curLoadout.HeavyShotgun);
            packetLoadout.AutoShotgunData = WeaponDataFromModelData(curLoadout.AutoShotgun);
            packetLoadout.TacticalSmgData = WeaponDataFromModelData(curLoadout.TacticalSMG);
            packetLoadout.RapidfireSmgData = WeaponDataFromModelData(curLoadout.RapidfireSMG);
            packetLoadout.SuppressedSmgData = WeaponDataFromModelData(curLoadout.SuppressedSMG);
            packetLoadout.StandardArData = WeaponDataFromModelData(curLoadout.StandardAR);
            packetLoadout.SemiautoArData = WeaponDataFromModelData(curLoadout.SemiAutoAR);
            packetLoadout.BurstArData = WeaponDataFromModelData(curLoadout.BurstAR);
            packetLoadout.TacticalArData = WeaponDataFromModelData(curLoadout.TacticalAR);
            packetLoadout.SuppressedArData = WeaponDataFromModelData(curLoadout.SuppressedAR);
            packetLoadout.HeavyArData = WeaponDataFromModelData(curLoadout.HeavyAR);
            packetLoadout.HighcalMgData = WeaponDataFromModelData(curLoadout.HighcalMG);
            packetLoadout.RapidfireMgData = WeaponDataFromModelData(curLoadout.RapidfireMG);
            packetLoadout.SemiautoSniperData = WeaponDataFromModelData(curLoadout.SemiAutoSniper);
            packetLoadout.BoltactionSniperData = WeaponDataFromModelData(curLoadout.BoltActionSniper);
            packetLoadout.MeleeData = WeaponDataFromModelData(curLoadout.Melee);
            responseData.WeaponLoadoutData.Add(packetLoadout);
        }
        return SpectreWebsocketMessage.From(responseData);
    }
}