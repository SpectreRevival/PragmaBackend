using Google.Protobuf;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class GetPlayerClientData : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetPlayerClientData(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnPlayerDataServiceRpc.GetAllPlayerDataClientV1Request");
    }

    public static async Task<FlatInstancedItem> GetFlatInstancedItem(Guid instanceId)
    {
        Model.CustomizedInstancedItem item = await Model.CustomizedInstancedItem.RetrieveFromDatabase(instanceId);
        FlatInstancedItem packet = new()
        {
            ItemInstanceId = item.InstanceId.ToString(),
            ItemCatalogId = item.CatalogId
        };
        return packet;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        FetchLoadoutsRequest req = Packet.GetPayloadAsMessage<FetchLoadoutsRequest>();
        Guid playerId = Guid.Parse(req.PlayerId);
        Model.ProfileData profile = await Model.ProfileData.RetrieveFromDatabase(playerId);
        Model.PlayerMatchmakingData mmData = await Model.PlayerMatchmakingData.RetrieveFromDatabase(playerId);
        PlayerClientData finalRes = new();
        PlayerData res = new()
        {
            PlayerId = req.PlayerId,
            AttackerOutfitLoadoutId = profile.AttackerOutfitLoadoutId.ToString(),
            AttackerWeaponLoadoutId = profile.AttackerWeaponLoadoutId.ToString(),
            DefenderWeaponLoadoutId = profile.DefenderWeaponLoadoutId.ToString(),
            DefenderOutfitLoadoutId = profile.DefenderOutfitLoadoutId.ToString(),
            PlayerFlags = profile.PlayerFlags
        };
        PlayerServiceData serviceData = new();
        res.ServerData = "{}";
        res.PlayerServiceData = serviceData;
        res.LastUpdated = profile.LastUpdated.ToString("yyyy-MM-ddTHH:mm");
        res.LastLogin = profile.LastLogin.ToUnixTimeMilliseconds().ToString();
        res.PostSpray = await GetFlatInstancedItem(profile.PostSprayItemId);
        res.MatchSpray = await GetFlatInstancedItem(profile.MatchSprayItemId);
        res.PreSpray = await GetFlatInstancedItem(profile.PreSprayItemId);
        res.Banner = await GetFlatInstancedItem(profile.BannerItemId);
        res.MatchmakingData = mmData.ToPacket();
        Model.PlayerConfig cfg = await Model.PlayerConfig.RetrieveFromDatabase(playerId);
        res.Data = await cfg.ToPacketFull(playerId);
        finalRes.Data = res;
        JsonFormatter outFormatter = new(
            new JsonFormatter.Settings(true)
            .WithFormatDefaultValues(true)
            .WithFormatEnumsAsIntegers(true)
            .WithPreserveProtoFieldNames(true)
        );
        string playerConfigJsonString = outFormatter.Format(res.Data);
        playerConfigJsonString = playerConfigJsonString.Replace(" ", "");
        string jsonString = outFormatter.Format(finalRes);
        jsonString = jsonString.Replace(" ", "");
        string[] strings1 = jsonString.Split("\"data\":{\"unlockAll"); // strings1[0] contains everything before the data property
        string[] afterstring = strings1[1].Split("\"matchmakingData\""); //afterstring[1] contains everything after the data property
        playerConfigJsonString = playerConfigJsonString.Replace("\r", "");
        playerConfigJsonString = playerConfigJsonString.Replace("\n", "");
        playerConfigJsonString = playerConfigJsonString.Replace("\"", "\\\"");
        string finalString = strings1[0] + $"\"data\":\"{playerConfigJsonString}\"," + "\"matchmakingData\"" + afterstring[1];
        return SpectreWebsocketMessage.From(finalString);
    }
}