#pragma once
#include <PacketProcessor.h>

class UpdatePresenceForPlayerHandler : public WebsocketPacketProcessor {
  public:
    explicit UpdatePresenceForPlayerHandler(SpectreRpcType rpcType);
    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};