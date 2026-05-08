#include "PlayerDatabase.h"
#include "PlayerPresence.pb.h"
#include "SetPlayerPresenceHandler.h"

#include <UpdatePlayerPresenceRequest.pb.h>
#include <UpdatePresenceForPlayerHandler.h>

UpdatePresenceForPlayerHandler::UpdatePresenceForPlayerHandler(SpectreRpcType rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> UpdatePresenceForPlayerHandler::Process(SpectreWebsocketRequest& packet) {
    std::unique_ptr<UpdatePlayerPresenceRequest> request = packet.GetPayloadAsMessage<UpdatePlayerPresenceRequest>();
    std::unique_ptr<PlayerPresence> playerPresence = PlayerDatabase::Get().GetField<PlayerPresence>(FieldKey::PLAYER_PRESENCE, packet.GetPlayerId());
    playerPresence->mutable_ext()->set_mainpresenceid(request->playerpresence().mainpresenceid());
    playerPresence->mutable_ext()->set_lastupdated(std::to_string(std::chrono::system_clock::now().time_since_epoch().count()));
    playerPresence->mutable_ext()->set_presencecontext(request->playerpresence().presencecontext());
    PlayerDatabase::Get().SetField(FieldKey::PLAYER_PRESENCE, playerPresence.get(), packet.GetPlayerId());
    nlohmann::json res{};
    res["success"] = true;
    return WebsocketPayload(res, {CreatePresenceNotification(*playerPresence, packet.GetPlayerId())});
}