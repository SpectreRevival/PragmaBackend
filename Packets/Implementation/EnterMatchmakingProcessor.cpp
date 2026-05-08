#include <EnterMatchmakingProcessor.h>
#include <EnterMatchmakingRequest.pb.h>
#include <PartyDatabase.h>

EnterMatchmakingProcessor::EnterMatchmakingProcessor(const SpectreRpcType& rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> EnterMatchmakingProcessor::Process(SpectreWebsocketRequest& packet) {
    const std::unique_ptr<EnterMatchmakingRequest> req = packet.GetPayloadAsMessage<EnterMatchmakingRequest>();
    const PartyResponse res = PartyDatabase::Get().GetPartyRes(req->partyid());
    return PartyDatabase::SerializePartyToString(res);
}