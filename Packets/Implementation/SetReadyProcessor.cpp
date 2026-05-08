#include <PartyDatabase.h>
#include <PlayerDatabase.h>
#include <SetReadyMessage.pb.h>
#include <SetReadyProcessor.h>

SetReadyProcessor::SetReadyProcessor(SpectreRpcType rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> SetReadyProcessor::Process(SpectreWebsocketRequest& packet) {
    std::unique_ptr<SetReadyMessage> readymsg = packet.GetPayloadAsMessage<SetReadyMessage>();
    PartyResponse res = PartyDatabase::Get().GetPartyRes(readymsg->partyid());
    bool playerFound = false;
    for (int i = 0; i < res.party().partymembers_size(); i++) {
        if (res.party().partymembers(i).playerid() == packet.GetPlayerId()) {
            res.mutable_party()->mutable_partymembers(i)->set_isready(readymsg->ready());
            playerFound = true;
            break;
        }
    }
    if (!playerFound) {
        spdlog::warn("Didn't find player to ready up, ignoring and returning party response\nParty ID: {}\nPlayer ID: {}", readymsg->partyid(), packet.GetPlayerId());
    }
    PartyDatabase::Get().SaveParty(res.party());
    return PartyDatabase::SerializePartyToString(res);
}