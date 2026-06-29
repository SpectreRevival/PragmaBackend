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
        res.Rank = rankedData.ToPacket();
        res.Casual = casualData.ToPacket();
        res.Team = teamData.ToPacket();
        return SpectreWebsocketMessage.From(res);
    }
}