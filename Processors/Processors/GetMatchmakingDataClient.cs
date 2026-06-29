using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

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
        PlayerMatchmakingData res = new()
        {
            PlayerId = req.PlayerId,
            MatchmakingFlags = 1.0,
            CasualMmr = mmData.CasualMMR,
            RankedMmr = mmData.RankedMMR,
            SoloRankPoints = mmData.SoloRankPoints,
            CasualMatchesPlayedCount = mmData.CasualMatchesPlayed,
            RankedMatchesPlayedCount = mmData.RankedMatchesPlayed,
            CasualMatchesPlayedSeasonCount = mmData.CasualMatchesPlayedSeasonal,
            RankedMatchesPlayedSeasonCount = mmData.RankedMatchesPlayedSeasonal,
            CurrentSoloRank = mmData.CurrentSoloRank,
            HighestTeamRank = mmData.HighestTeamRank,
            CasualMatchesWonCount = mmData.CasualMatchesWon,
            RankedMatchesWonCount = mmData.RankedMatchesWon,
            PriorityMatchmakingUntil = mmData.PriorityMatchmakingUntil.ToUnixTimeMilliseconds().ToString(),
            RestrictMatchmakingUntil = mmData.RestrictMatchmakingUntil.ToUnixTimeMilliseconds().ToString(),
            MapHistory = mmData.MapHistory
        };
        foreach (string item in mmData.RankedPlacementMatches)
        {
            res.RankedPlacementMatches.Add(item);
        }
        finalRes.Data = res;
        return SpectreWebsocketMessage.From(finalRes);
    }
}