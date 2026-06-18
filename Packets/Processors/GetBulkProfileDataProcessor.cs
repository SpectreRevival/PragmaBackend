using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

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
            ProfileData packet = new();
            packet.PlayerId = playerId;
            DisplayName displayName = new();
            displayName.DisplayName_ = profileData.DisplayName.PlayerName;
            displayName.Discriminator = profileData.DisplayName.Discriminator;
            packet.DisplayName = displayName;
            packet.CrewScore = profileData.CrewScore.ToString();
            packet.CurrentSoloRank = profileData.CurrentSoloRank;
            packet.HighestTeamRank = profileData.HighestTeamRank;
            packet.DivisionType = profileData.DivisionType;
            FlatInstancedItem bannerItem = new();
            bannerItem.ItemInstanceId = profileData.BannerItemId.ToString();
            Model.CustomizedInstancedItem? bannerFullItem = await Model.CustomizedInstancedItem.RetrieveFromDatabase(profileData.BannerItemId);
            if(bannerFullItem == null)
            {
                throw new InvalidDataException($"Couldn't find CustomizedInstancedItem associated with bannerItemId in profile data {profileData.BannerItemId}");
            }
            bannerItem.ItemCatalogId = bannerFullItem.CatalogId;
            packet.Banner = bannerItem;
            res.BulkProfileData.Add(packet);
        }
        return SpectreWebsocketMessage.From(res);
    }
}