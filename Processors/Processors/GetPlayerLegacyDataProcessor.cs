using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

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
        LegacyStatsData ret = new()
        {
            KillCount = data.KillCount,
            DeathCount = data.DeathCount,
            AceCount = data.AceCount,
            DualityKillCount = data.DualityKillCount,
            FirstKillCount = data.FirstKillCount,
            FirstDeathCount = data.FirstDeathCount,
            Kast = data.KAST,
            DualityRating = data.DualityRating,
            ImpactCount = data.ImpactCount,
            TotalMatchesPlayedCount = data.TotalMatchesPlayedCount,
            FanCount = data.FanCount,
            WinCount = data.WinCount,
            TotalRoundsPlayedCount = data.TotalRoundsPlayedCount,
            HeadshotsCount = data.HeadshotsCount,
            TotalDamagingShotsCount = data.TotalDamagingShotsCount,
            TotalDamageCount = data.TotalDamageCount
        };
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
        LegacySeasonData packetSeason = new()
        {
            CurrentSoloRank = seasonData.CurrentSoloRank,
            SoloRankPoints = seasonData.SoloRankedPoints
        };
        res.SeasonData = packetSeason;
        res.Rank = ConvertLegacyStatsData(rankedData);
        res.Casual = ConvertLegacyStatsData(casualData);
        res.Team = ConvertLegacyStatsData(teamData);
        return SpectreWebsocketMessage.From(res);
    }
}