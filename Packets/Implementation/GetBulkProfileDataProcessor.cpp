#include <BulkProfileDataMessage.pb.h>
#include <GetBulkProfileDataProcessor.h>
#include <PlayerDatabase.h>
#include <ProfileData.pb.h>

GetBulkProfileDataProcessor::GetBulkProfileDataProcessor(const SpectreRpcType& rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> GetBulkProfileDataProcessor::Process(SpectreWebsocketRequest& packet) {
    const std::unique_ptr<GetBulkProfileDataMessage> req = packet.GetPayloadAsMessage<GetBulkProfileDataMessage>();
    BulkProfileDataResponse res;
    for (int i = 0; i < req->playerids_size(); i++) {
        res.add_bulkprofiledata()->CopyFrom(
            *PlayerDatabase::Get().GetField<ProfileData>(FieldKey::PROFILE_DATA, req->playerids(i)));
    }
    return res;
}