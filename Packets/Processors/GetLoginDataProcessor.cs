using Model;
using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

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

    private static PlayerMatchmakingData MatchmakingDataConvert(Model.PlayerMatchmakingData mm)
    {
        PlayerMatchmakingData packet = new();
        packet.PlayerId = mm.PlayerId.ToString();
        packet.CasualMmr = mm.CasualMMR;
        packet.RankedMmr = mm.RankedMMR;
        packet.SoloRankPoints = mm.SoloRankPoints;
        packet.CasualMatchesPlayedCount = mm.CasualMatchesPlayed;
        packet.RankedMatchesPlayedCount = mm.RankedMatchesPlayed;
        packet.CasualMatchesPlayedSeasonCount = mm.CasualMatchesPlayedSeasonal;
        packet.RankedMatchesPlayedSeasonCount = mm.RankedMatchesPlayedSeasonal;
        foreach (string placementMatch in mm.RankedPlacementMatches)
        {
            packet.RankedPlacementMatches.Add(placementMatch);
        }
        packet.CurrentSoloRank = mm.CurrentSoloRank;
        packet.HighestTeamRank = mm.HighestTeamRank;
        packet.CasualMatchesWonCount = mm.CasualMatchesWon;
        packet.RankedMatchesWonCount = mm.RankedMatchesWon;
        packet.PriorityMatchmakingUntil = mm.PriorityMatchmakingUntil.ToUnixTimeMilliseconds().ToString();
        packet.RestrictMatchmakingUntil = mm.RestrictMatchmakingUntil.ToUnixTimeMilliseconds().ToString();
        packet.MatchmakingFlags = 1.0;
        packet.MapHistory = mm.MapHistory;
        return packet;
    }

    private static async Task<FlatInstancedItem> BannerConvert(Guid instanceId)
    {
        Model.CustomizedInstancedItem item = await Model.CustomizedInstancedItem.RetrieveFromDatabase(instanceId);
        FlatInstancedItem packet = new();
        packet.ItemInstanceId = item.InstanceId.ToString();
        packet.ItemCatalogId = item.CatalogId;
        return packet;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        LoginDataResponse res = new();
        LoginData loginData = new();
        LoginDataExt ext = new();
        ext.CrewAutomationInProcess = false;
        ext.CurrentServiceTimestampMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        ext.NoCrew = -1.0d;
        ext.NextCrewAutomationDate = DateTimeOffset.MinValue.ToString("yyyy-MM-ddTHH:mm");
        PlayerData playerData = new();
        Model.ProfileData profileData = await Model.ProfileData.RetrieveFromDatabase(ConnectionHandler.PlayerId);
        playerData.PlayerId = ConnectionHandler.PlayerId.ToString();
        playerData.AttackerOutfitLoadoutId = profileData.AttackerOutfitLoadoutId.ToString();
        playerData.AttackerWeaponLoadoutId = profileData.AttackerWeaponLoadoutId.ToString();
        playerData.DefenderOutfitLoadoutId = profileData.DefenderOutfitLoadoutId.ToString();
        playerData.DefenderWeaponLoadoutId = profileData.DefenderWeaponLoadoutId.ToString();
        playerData.LastUpdated = profileData.LastUpdated.ToString();
        playerData.PlayerFlags = profileData.PlayerFlags;
        playerData.LastLogin = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        playerData.ServerData = "{}";
        Model.PlayerMatchmakingData mmData = await Model.PlayerMatchmakingData.RetrieveFromDatabase(ConnectionHandler.PlayerId);
        playerData.MatchmakingData = MatchmakingDataConvert(mmData);
        playerData.Banner = await BannerConvert(profileData.BannerItemId);
        playerData.PreSpray = await BannerConvert(profileData.PreSprayItemId);
        playerData.MatchSpray = await BannerConvert(profileData.MatchSprayItemId);
        playerData.PostSpray = await BannerConvert(profileData.PostSprayItemId);
        PlayerServiceData playerServiceData = new();
        playerData.PlayerServiceData = playerServiceData;
        ext.PlayerData = playerData;
        loginData.Ext = ext;
        res.LoginData = loginData;
        throw new NotImplementedException();
    }
}