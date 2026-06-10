using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

public class GetPlayerLegacyDataProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetPlayerLegacyDataProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnPlayerDataServiceRpc.GetPlayerLegacyDataV1Request");
    }

    private static LegacyStatsData ConvertLegacyStatsData(Model.LegacyStatsData data)
    {
        LegacyStatsData ret = new();
        ret.KillCount = data.KillCount;
        ret.DeathCount = data.DeathCount;
        ret.AceCount = data.AceCount;
        ret.DualityKillCount = data.DualityKillCount;
        ret.FirstKillCount = data.FirstKillCount;
        ret.FirstDeathCount = data.FirstDeathCount;
        ret.Kast = data.KAST;
        ret.DualityRating = data.DualityRating;
        ret.ImpactCount = data.ImpactCount;
        ret.TotalMatchesPlayedCount = data.TotalMatchesPlayedCount;
        ret.FanCount = data.FanCount;
        ret.WinCount = data.WinCount;
        ret.TotalRoundsPlayedCount = data.TotalRoundsPlayedCount;
        ret.HeadshotsCount = data.HeadshotsCount;
        ret.TotalDamagingShotsCount = data.TotalDamagingShotsCount;
        ret.TotalDamageCount = data.TotalDamageCount;
        foreach (string sponsor in data.TopSponsors)
        {
            ret.Sponsors.Add(sponsor);
        }
        foreach (string weapon in data.TopWeapons)
        {
            ret.Weapons.Add(weapon);
        }
        return ret;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        Model.LegacySeasonData seasonData = await Model.LegacySeasonData.RetrieveFromDatabase(ConnectionHandler.PlayerId);
        Model.LegacyStatsData casualData = await Model.LegacyStatsData.RetrieveFromDatabase(new Model.LegacyStatsDataKey(ConnectionHandler.PlayerId, Model.LegacyStatsType.Casual));
        Model.LegacyStatsData rankedData = await Model.LegacyStatsData.RetrieveFromDatabase(new Model.LegacyStatsDataKey(ConnectionHandler.PlayerId, Model.LegacyStatsType.Ranked));
        Model.LegacyStatsData teamData = await Model.LegacyStatsData.RetrieveFromDatabase(new Model.LegacyStatsDataKey(ConnectionHandler.PlayerId, Model.LegacyStatsType.Team));

        LegacyPlayerData res = new();
        LegacySeasonData packetSeason = new();
        packetSeason.CurrentSoloRank = seasonData.CurrentSoloRank;
        packetSeason.SoloRankPoints = seasonData.SoloRankedPoints;
        res.SeasonData = packetSeason;
        res.Rank = ConvertLegacyStatsData(rankedData);
        res.Casual = ConvertLegacyStatsData(casualData);
        res.Team = ConvertLegacyStatsData(teamData);
        return SpectreWebsocketMessage.From(res);
    }
}