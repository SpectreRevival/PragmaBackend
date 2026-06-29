using Google.Protobuf;
using Model;
using Packets;
using System.Diagnostics.CodeAnalysis;
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

    private static async Task ConvertAndSaveItem(FlatInstancedItem item, Guid playerId)
    {
        CustomizedInstancedItem newItem = new(playerId, item.ItemCatalogId, Guid.Parse(item.ItemInstanceId), true, []);
        await newItem.SyncToDatabase();
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        string innerJson = Packet.RequestPayload["data"].ToString();
        innerJson = innerJson.Replace("\\\"", "\"");
        innerJson = innerJson.Replace("\\r", "");
        innerJson = innerJson.Replace("\\n", "");
        innerJson = innerJson.Replace("\\t", "");
        innerJson = innerJson.Replace("\\u0022", "\"");
        innerJson = innerJson.Replace("\\u002B", "+");
        Packet.RequestPayload["data"] = JsonNode.Parse(innerJson);
        JsonParser parser = new(JsonParser.Settings.Default);
        PlayerData packetData = parser.Parse<PlayerData>(Packet.RequestPayload.ToJsonString());
        Guid playerId = Guid.Parse(packetData.PlayerId);
        Model.ProfileData profile = await Model.ProfileData.RetrieveFromDatabase(playerId);
        if (packetData.Banner.ItemCatalogId != "" && packetData.Banner.ItemInstanceId != "")
        {
            await ConvertAndSaveItem(packetData.Banner, playerId);
            profile.BannerItemId = Guid.Parse(packetData.Banner.ItemInstanceId);
        }
        if (packetData.PreSpray.ItemCatalogId != "" && packetData.PreSpray.ItemInstanceId != "")
        {
            await ConvertAndSaveItem(packetData.PreSpray, playerId);
            profile.PreSprayItemId = Guid.Parse(packetData.PreSpray.ItemInstanceId);
        }
        if (packetData.MatchSpray.ItemCatalogId != "" && packetData.MatchSpray.ItemInstanceId != "")
        {
            await ConvertAndSaveItem(packetData.MatchSpray, playerId);
            profile.MatchSprayItemId = Guid.Parse(packetData.MatchSpray.ItemInstanceId);
        }
        if (packetData.PostSpray.ItemCatalogId != "" && packetData.PostSpray.ItemInstanceId != "")
        {
            await ConvertAndSaveItem(packetData.PostSpray, playerId);
            profile.PostSprayItemId = Guid.Parse(packetData.PostSpray.ItemInstanceId);
        }

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

        return SpectreWebsocketMessage.From("{\"success\":true}");
    }
}