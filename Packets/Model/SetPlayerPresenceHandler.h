#pragma once
#include "PlayerPresence.pb.h"

#include <PacketProcessor.h>

class SetPlayerPresenceHandler : public WebsocketPacketProcessor {
  public:
    explicit SetPlayerPresenceHandler(SpectreRpcType rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};

void UpdatePlayerPresence(PlayerPresence& newPresence, const std::string& playerId);