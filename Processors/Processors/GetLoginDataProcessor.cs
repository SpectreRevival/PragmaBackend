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

    private static Packets.PlayerMatchmakingData MatchmakingDataConvert(Model.PlayerMatchmakingData mm)
    {
        Packets.PlayerMatchmakingData packet = new()
        {
            PlayerId = mm.PlayerId.ToString(),
            CasualMmr = mm.CasualMMR,
            RankedMmr = mm.RankedMMR,
            SoloRankPoints = mm.SoloRankPoints,
            CasualMatchesPlayedCount = mm.CasualMatchesPlayed,
            RankedMatchesPlayedCount = mm.RankedMatchesPlayed,
            CasualMatchesPlayedSeasonCount = mm.CasualMatchesPlayedSeasonal,
            RankedMatchesPlayedSeasonCount = mm.RankedMatchesPlayedSeasonal
        };
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

    private static async Task<FlatInstancedItem> GetFlatInstancedItem(Guid instanceId)
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
        LoginDataResponse res = new();
        LoginData loginData = new();
        LoginDataExt ext = new()
        {
            CrewAutomationInProcess = false,
            CurrentServiceTimestampMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            NoCrew = -1.0d,
            NextCrewAutomationDate = DateTimeOffset.MinValue.ToString("yyyy-MM-ddTHH:mm")
        };
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
        playerData.Banner = await GetFlatInstancedItem(profileData.BannerItemId);
        playerData.PreSpray = await GetFlatInstancedItem(profileData.PreSprayItemId);
        playerData.MatchSpray = await GetFlatInstancedItem(profileData.MatchSprayItemId);
        playerData.PostSpray = await GetFlatInstancedItem(profileData.PostSprayItemId);
        PlayerServiceData playerServiceData = new();
        playerData.PlayerServiceData = playerServiceData;
        ext.PlayerData = playerData;
        loginData.Ext = ext;
        res.LoginData = loginData;
        throw new NotImplementedException();
    }
}