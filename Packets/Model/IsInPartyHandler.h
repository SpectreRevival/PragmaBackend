#pragma once
#include <PacketProcessor.h>

class IsInPartyHandler : public WebsocketPacketProcessor {
  public:
    explicit IsInPartyHandler(const SpectreRpcType& rpcType);
    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};