using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

public class GetMatchmakingClientData : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetMatchmakingClientData(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnPlayerDataServiceRpc.GetMatchmakingDataClientV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        FetchLoadoutsRequest req = Packet.GetPayloadAsMessage<FetchLoadoutsRequest>();
        Model.PlayerMatchmakingData mmData = await Model.PlayerMatchmakingData.RetrieveFromDatabase(Guid.Parse(req.PlayerId));
        PlayerMatchmakingDataResponse finalRes = new();
        PlayerMatchmakingData res = new();
        res.PlayerId = req.PlayerId;
        res.MatchmakingFlags = 1.0;
        res.CasualMmr = mmData.CasualMMR;
        res.RankedMmr = mmData.RankedMMR;
        res.SoloRankPoints = mmData.SoloRankPoints;
        res.CasualMatchesPlayedCount = mmData.CasualMatchesPlayed;
        res.RankedMatchesPlayedCount = mmData.RankedMatchesPlayed;
        res.CasualMatchesPlayedSeasonCount = mmData.CasualMatchesPlayedSeasonal;
        res.RankedMatchesPlayedSeasonCount = mmData.RankedMatchesPlayedSeasonal;
        res.CurrentSoloRank = mmData.CurrentSoloRank;
        res.HighestTeamRank = mmData.HighestTeamRank;
        res.CasualMatchesWonCount = mmData.CasualMatchesWon;
        res.RankedMatchesWonCount = mmData.RankedMatchesWon;
        res.PriorityMatchmakingUntil = mmData.PriorityMatchmakingUntil.ToUnixTimeMilliseconds().ToString();
        res.RestrictMatchmakingUntil = mmData.RestrictMatchmakingUntil.ToUnixTimeMilliseconds().ToString();
        res.MapHistory = mmData.MapHistory;
        foreach (var item in mmData.RankedPlacementMatches)
        {
            res.RankedPlacementMatches.Add(item);
        }
        finalRes.Data = res;
        return SpectreWebsocketMessage.From(finalRes);
    }
}