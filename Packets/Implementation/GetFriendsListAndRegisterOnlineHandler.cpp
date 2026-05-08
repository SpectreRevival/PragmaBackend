#include "PlayerDatabase.h"
#include "SetPlayerPresenceHandler.h"

#include <FriendsList.pb.h>
#include <GetFriendsListAndRegisterOnlineHandler.h>
#include <GetFriendsListAndRegisterOnlineResponse.pb.h>
#include <PlayerPresence.pb.h>

GetFriendsListAndRegisterOnlineHandler::GetFriendsListAndRegisterOnlineHandler(SpectreRpcType rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> GetFriendsListAndRegisterOnlineHandler::Process(SpectreWebsocketRequest& packet) {
    std::unique_ptr<FriendsList> friendsList = PlayerDatabase::Get().GetField<FriendsList>(FieldKey::FRIENDS_LIST, packet.GetPlayerId());
    std::unique_ptr<PlayerPresence> presence = PlayerDatabase::Get().GetField<PlayerPresence>(FieldKey::PLAYER_PRESENCE, packet.GetPlayerId());
    presence->set_basicpresence(Online);
    presence->set_version(std::to_string(std::stoi(presence->version()) + 1));
    PlayerDatabase::Get().SetField(FieldKey::PLAYER_PRESENCE, presence.get(), packet.GetPlayerId());
    GetFriendsListAndRegisterOnlineResponse response;
    response.mutable_friendlist()->CopyFrom(*friendsList);
    return WebsocketPayload(response, {CreatePresenceNotification(*presence, packet.GetPlayerId())});
}