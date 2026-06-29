using Model.Persistence;
using Npgsql;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

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

    private static Packets.WeaponData WeaponDataFromModelData(Model.WeaponData weaponData)
    {
        Packets.WeaponData data = new()
        {
            ItemInstanceId = weaponData.ItemInstanceId.ToString(),
            ItemCatalogId = weaponData.ItemCatalogId
        };
        if (weaponData.Attachment != null)
        {
            data.AttachmentItemInstanceId = weaponData.Attachment.AttachmentItemInstanceId.ToString();
            data.AttachmentItemCatalogId = weaponData.Attachment.AttachmentItemCatalogId;
        }
        foreach (Model.ActiveAlterationData altData in weaponData.AlterationData)
        {
            Packets.ActiveAlterationData packetAltData = new()
            {
                AlterationId = altData.AlterationId,
                ChannelId = altData.ChannelId
            };
            data.AlterationData.Add(packetAltData);
        }
        return data;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        FetchLoadoutsRequest req = Packet.GetPayloadAsMessage<FetchLoadoutsRequest>();
        NpgsqlCommand cmd = PostgresDatabase.Get().GetRaw().CreateCommand("SELECT loadout_id FROM weapon_loadouts WHERE player_id = @player_id");
        cmd.Parameters.AddWithValue("player_id", Guid.Parse(req.PlayerId));
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        WeaponLoadouts responseData = new();
        while (await reader.ReadAsync())
        {
            Guid loadoutId = reader.GetGuid(0);
            Model.WeaponLoadout curLoadout = await Model.WeaponLoadout.RetrieveFromDatabase(loadoutId);
            Packets.WeaponLoadout packetLoadout = new()
            {
                PlayerId = req.PlayerId,
                LoadoutId = curLoadout.LoadoutId.ToString(),
                SemiautoPistolData = WeaponDataFromModelData(curLoadout.SemiAutoPistol),
                SuppressedPistolData = WeaponDataFromModelData(curLoadout.SuppressedPistol),
                AutoPistolData = WeaponDataFromModelData(curLoadout.AutoPistol),
                HighcalPistolData = WeaponDataFromModelData(curLoadout.HighcalPistol),
                HeavyShotgunData = WeaponDataFromModelData(curLoadout.HeavyShotgun),
                AutoShotgunData = WeaponDataFromModelData(curLoadout.AutoShotgun),
                TacticalSmgData = WeaponDataFromModelData(curLoadout.TacticalSMG),
                RapidfireSmgData = WeaponDataFromModelData(curLoadout.RapidfireSMG),
                SuppressedSmgData = WeaponDataFromModelData(curLoadout.SuppressedSMG),
                StandardArData = WeaponDataFromModelData(curLoadout.StandardAR),
                SemiautoArData = WeaponDataFromModelData(curLoadout.SemiAutoAR),
                BurstArData = WeaponDataFromModelData(curLoadout.BurstAR),
                TacticalArData = WeaponDataFromModelData(curLoadout.TacticalAR),
                SuppressedArData = WeaponDataFromModelData(curLoadout.SuppressedAR),
                HeavyArData = WeaponDataFromModelData(curLoadout.HeavyAR),
                HighcalMgData = WeaponDataFromModelData(curLoadout.HighcalMG),
                RapidfireMgData = WeaponDataFromModelData(curLoadout.RapidfireMG),
                SemiautoSniperData = WeaponDataFromModelData(curLoadout.SemiAutoSniper),
                BoltactionSniperData = WeaponDataFromModelData(curLoadout.BoltActionSniper),
                MeleeData = WeaponDataFromModelData(curLoadout.Melee)
            };
            responseData.WeaponLoadoutData.Add(packetLoadout);
        }
        return SpectreWebsocketMessage.From(responseData);
    }
}