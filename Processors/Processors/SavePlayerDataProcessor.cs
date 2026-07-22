using Model;
using Packets;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public class SavePlayerDataProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public SavePlayerDataProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnPlayerDataServiceRpc.SavePlayerDataForClientV1Request");
    }

    // the client only ever fills ItemInstanceId on these slots, never ItemCatalogId, so a slot
    // must be accepted on the instance alone or every banner/spray equip is silently dropped.
    // returns null to mean "leave the stored value alone" - an empty slot is what the client
    // sends when it could not resolve the instance locally, not a request to clear the cosmetic.
    private static async Task<Guid?> ResolveCosmeticSlot(FlatInstancedItem item, Guid playerId, string slotName)
    {
        if (!Guid.TryParse(item.ItemInstanceId, out Guid instanceId) || instanceId == Guid.Empty)
        {
            return null;
        }

        CustomizedInstancedItem? owned = await CustomizedInstancedItem.RetrieveFromDatabase(instanceId);
        if (owned is null || owned.OwningPlayerId != playerId)
        {
            Log.Warning("SavePlayerData: player {PlayerId} tried to equip {Slot} instance {InstanceId} they do not own, keeping previous value",
                playerId, slotName, instanceId);
            return null;
        }
        return instanceId;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        try
        {
            return await ProcessSave(Packet);
        }
        catch (Exception)
        {
            Log.Warning("SAVEDIAG: SavePlayerData raw={Raw}", Packet.RequestPayload.ToJsonString());
            throw;
        }
    }

    private static async Task<SpectreWebsocketMessage> ProcessSave(SpectreWebsocketRequest Packet)
    {
        JsonNode dataNode = Packet.RequestPayload["data"] ?? throw new InvalidDataException("SavePlayerData request had no data field");
        string innerJson = dataNode.GetValueKind() == JsonValueKind.String ? dataNode.GetValue<string>() : dataNode.ToJsonString();
        Packet.RequestPayload["data"] = JsonNode.Parse(innerJson);
        PlayerData packetData = Packet.GetPayloadAsMessage<PlayerData>();
        Guid playerId = Guid.Parse(packetData.PlayerId);
        Model.ProfileData profile = await Model.ProfileData.RetrieveFromDatabase(playerId);
        Guid previousBanner = profile.BannerItemId;
        profile.BannerItemId = await ResolveCosmeticSlot(packetData.Banner, playerId, "Banner") ?? profile.BannerItemId;
        profile.PreSprayItemId = await ResolveCosmeticSlot(packetData.PreSpray, playerId, "PreSpray") ?? profile.PreSprayItemId;
        profile.MatchSprayItemId = await ResolveCosmeticSlot(packetData.MatchSpray, playerId, "MatchSpray") ?? profile.MatchSprayItemId;
        profile.PostSprayItemId = await ResolveCosmeticSlot(packetData.PostSpray, playerId, "PostSpray") ?? profile.PostSprayItemId;
        bool bannerChanged = profile.BannerItemId != previousBanner;

        if (packetData.AttackerOutfitLoadoutId != "")
        {
            profile.AttackerOutfitLoadoutId = Guid.Parse(packetData.AttackerOutfitLoadoutId);
        }

        if (packetData.AttackerWeaponLoadoutId != "")
        {
            profile.AttackerWeaponLoadoutId = Guid.Parse(packetData.AttackerWeaponLoadoutId);
        }

        if (packetData.DefenderOutfitLoadoutId != "")
        {
            profile.DefenderOutfitLoadoutId = Guid.Parse(packetData.DefenderOutfitLoadoutId);
        }

        if (packetData.DefenderWeaponLoadoutId != "")
        {
            profile.DefenderWeaponLoadoutId = Guid.Parse(packetData.DefenderWeaponLoadoutId);
        }

        profile.PlayerFlags = (int)Math.Round(packetData.PlayerFlags);

        Model.PlayerConfig playerCfg = Model.PlayerConfig.FromPacket(packetData.Data, playerId);
        await playerCfg.SyncToDatabase();
        await Model.CrosshairConfig.FromPacket(packetData.Data.CrosshairConfig, playerId).SyncToDatabase();
        await Model.SubtitleUserSettings.FromPacket(packetData.Data.SubtitleUserSettings, playerId).SyncToDatabase();
        await Model.ColorVisionConfig.FromPacket(packetData.Data.ColorVisionConfig, playerId).SyncToDatabase();
        await Model.GamepadConfig.FromPacket(packetData.Data.GamepadConfig, playerId).SyncToDatabase();

        await profile.SyncToDatabase();

        Log.Information("SavePlayerData: saved for {PlayerId}", playerId);
        return SpectreWebsocketMessage.From("{\"success\":true}");
    }
}