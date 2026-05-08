#include "PresenceUpdateNotification.pb.h"
#include "SpectreWebsocket.h"

#include <PlayerDatabase.h>
#include <SetPlayerPresenceHandler.h>
#include <SetPresenceRequest.pb.h>

SetPlayerPresenceHandler::SetPlayerPresenceHandler(SpectreRpcType rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> SetPlayerPresenceHandler::Process(SpectreWebsocketRequest& packet) {
    std::unique_ptr<SetPresenceRequest> req = packet.GetPayloadAsMessage<SetPresenceRequest>();

    std::unique_ptr<PlayerPresence> presence = PlayerDatabase::Get().GetField<PlayerPresence>(FieldKey::PLAYER_PRESENCE, packet.GetPlayerId());
    presence->set_basicpresence(req->basicpresence());
    int version = std::stoi(presence->version());
    version++;
    presence->set_version(std::to_string(version));
    PlayerDatabase::Get().SetField(FieldKey::PLAYER_PRESENCE, presence.get(), packet.GetPlayerId());
    nlohmann::json res{};
    res["response"] = "Ok";
    return WebsocketPayload(res, {CreatePresenceNotification(*presence, packet.GetPlayerId())});
}

Notification CreatePresenceNotification(PlayerPresence& newPresence, const std::string& playerId) {
    PresenceUpdateNotification updateNotification;
    updateNotification.mutable_newpresence()->set_gameshardid("00000000-0000-0000-0000-000000000001");
    updateNotification.mutable_newpresence()->set_gametitleid("00000000-0000-0000-0000-000000000001");
    updateNotification.mutable_newpresence()->set_playerid(playerId);
    updateNotification.mutable_newpresence()->set_version(newPresence.version());
    updateNotification.mutable_newpresence()->set_basicpresence(newPresence.basicpresence());
    return Notification(SpectreRpcType("FriendRpc.PresenceUpdateV1Notification"), updateNotification);
}