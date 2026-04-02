#pragma once
#include <PacketProcessor.h>

class FetchPlayerLoadoutsProcessor : public WebsocketPacketProcessor {
  public:
    explicit FetchPlayerLoadoutsProcessor(const SpectreRpcType& rpcType);
    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};