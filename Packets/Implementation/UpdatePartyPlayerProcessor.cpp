#include <PartyDatabase.h>
#include <PartyMember.pb.h>
#include <UpdatePartyPlayerProcessor.h>
#include <UpdatePartyPlayerRequest.pb.h>

UpdatePartyPlayerProcessor::UpdatePartyPlayerProcessor(SpectreRpcType rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> UpdatePartyPlayerProcessor::Process(SpectreWebsocketRequest& packet) {
    std::unique_ptr<UpdatePartyPlayerRequest> req = packet.GetPayloadAsMessage<UpdatePartyPlayerRequest>();
    PartyResponse res = PartyDatabase::Get().GetPartyRes(req->partyid());
    Party* party = res.mutable_party();
    PartyMemberExtraInfo* memberExt = nullptr;
    for (int i = 0; i < party->partymembers_size(); i++) {
        if (party->partymembers(i).playerid() == packet.GetPlayerId()) {
            memberExt = party->mutable_partymembers(i)->mutable_ext();
        }
    }
    if (memberExt == nullptr) {
        spdlog::warn("could not find player(id: {}) in party(id: {}) that was being updated by UpdatePartyPlayerProcessor");
        return std::nullopt;
    }
    memberExt->set_version(req->requestext().version());
    memberExt->set_region(req->requestext().region());
    memberExt->set_preferredteam(req->requestext().preferredteam());
    // ignoring refreshData, preferredRegions, regionPings, sharedClientData for the moment as they don't seem to be important or used for customgame
    PartyDatabase::Get().SaveParty(res.party());
    return PartyDatabase::SerializePartyToString(res);
}