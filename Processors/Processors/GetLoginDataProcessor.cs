using Google.Protobuf;
using Model;
using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class GetLoginDataProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetLoginDataProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("GameDataRpc.GetLoginDataV3Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        Guid playerId = ConnectionHandler.PlayerId;
        Model.ProfileData profileData = await Model.ProfileData.RetrieveFromDatabase(playerId);
        Model.PlayerMatchmakingData mmData = await Model.PlayerMatchmakingData.RetrieveFromDatabase(playerId);

        LoginDataResponse res = new();
        LoginData loginData = new();
        LoginDataExt ext = new()
        {
            CrewAutomationInProcess = false,
            CurrentServiceTimestampMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            NoCrew = -1.0d,
            NextCrewAutomationDate = DateTimeOffset.MinValue.ToString("yyyy-MM-ddTHH:mm")
        };

        PlayerData playerData = new()
        {
            PlayerId = playerId.ToString(),
            AttackerOutfitLoadoutId = profileData.AttackerOutfitLoadoutId.ToString(),
            AttackerWeaponLoadoutId = profileData.AttackerWeaponLoadoutId.ToString(),
            DefenderOutfitLoadoutId = profileData.DefenderOutfitLoadoutId.ToString(),
            DefenderWeaponLoadoutId = profileData.DefenderWeaponLoadoutId.ToString(),
            LastUpdated = profileData.LastUpdated.ToString("yyyy-MM-ddTHH:mm"),
            LastLogin = profileData.LastLogin.ToUnixTimeMilliseconds().ToString(),
            PlayerFlags = profileData.PlayerFlags,
            ServerData = "{}",
            PlayerServiceData = new PlayerServiceData(),
            MatchmakingData = mmData.ToPacket(),
            Banner = await GetPlayerClientData.GetFlatInstancedItem(profileData.BannerItemId),
            PreSpray = await GetPlayerClientData.GetFlatInstancedItem(profileData.PreSprayItemId),
            MatchSpray = await GetPlayerClientData.GetFlatInstancedItem(profileData.MatchSprayItemId),
            PostSpray = await GetPlayerClientData.GetFlatInstancedItem(profileData.PostSprayItemId)
        };
        Model.PlayerConfig playerCfg = await Model.PlayerConfig.RetrieveFromDatabase(playerId);
        playerData.Data = await playerCfg.ToPacketFull(playerId);

        ext.PlayerData = playerData;
        loginData.Ext = ext;
        res.LoginData = loginData;

        JsonFormatter outFormatter = new(
            new JsonFormatter.Settings(true)
            .WithFormatDefaultValues(true)
            .WithFormatEnumsAsIntegers(true)
            .WithPreserveProtoFieldNames(true)
        );

        // the client wants playerData.data as stringified json, not a nested object. same shit as GetPlayerClientData.
        string playerConfigJson = outFormatter.Format(playerData.Data);
        playerConfigJson = playerConfigJson.Replace(" ", "");
        string jsonString = outFormatter.Format(res);
        jsonString = jsonString.Replace(" ", "");
        string[] beforeData = jsonString.Split("\"data\":{\"unlockAll");
        string[] afterData = beforeData[1].Split("\"matchmakingData\"");
        playerConfigJson = playerConfigJson.Replace("\r", "");
        playerConfigJson = playerConfigJson.Replace("\n", "");
        playerConfigJson = playerConfigJson.Replace("\"", "\\\"");
        string extOnly = beforeData[0] + $"\"data\":\"{playerConfigJson}\"," + "\"matchmakingData\"" + afterData[1];

        string finalString = extOnly[..^2] + ",\"inventoryData\":" + InventoryStore.Get().GetPacketString() + "}}";
        return SpectreWebsocketMessage.From(finalString);
    }
}
