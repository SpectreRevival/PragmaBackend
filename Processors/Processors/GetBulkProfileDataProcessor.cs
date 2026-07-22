using Packets;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class GetBulkProfileDataProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetBulkProfileDataProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnPlayerDataServiceRpc.GetBulkProfileDataClientV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        GetBulkProfileDataMessage req = Packet.GetPayloadAsMessage<GetBulkProfileDataMessage>();
        BulkProfileDataResponse res = new();
        foreach (string playerId in req.PlayerIds)
        {
            Model.ProfileData profileData = await Model.ProfileData.RetrieveFromDatabase(Guid.Parse(playerId));
            ProfileData packet = new()
            {
                PlayerId = playerId
            };
            DisplayName displayName = new()
            {
                DisplayName_ = profileData.DisplayName.PlayerName,
                Discriminator = profileData.DisplayName.Discriminator
            };
            packet.DisplayName = displayName;
            packet.CrewScore = profileData.CrewScore.ToString();
            packet.CurrentSoloRank = profileData.CurrentSoloRank;
            packet.HighestTeamRank = profileData.HighestTeamRank;
            packet.DivisionType = profileData.DivisionType;
            FlatInstancedItem bannerItem = new()
            {
                ItemInstanceId = profileData.BannerItemId.ToString()
            };
            Model.CustomizedInstancedItem? bannerFullItem = await Model.CustomizedInstancedItem.RetrieveFromDatabase(profileData.BannerItemId);
            if (bannerFullItem == null)
            {
                // one unresolvable banner used to throw and take the whole bulk response with it,
                // blanking every profile in the request
                Log.Warning("GetBulkProfileData: banner instance {InstanceId} for player {PlayerId} not found, sending empty banner",
                    profileData.BannerItemId, playerId);
                bannerItem.ItemInstanceId = "";
            }
            else
            {
                bannerItem.ItemCatalogId = bannerFullItem.CatalogId;
            }
            packet.Banner = bannerItem;
            res.BulkProfileData.Add(packet);
        }
        return SpectreWebsocketMessage.From(res);
    }
}