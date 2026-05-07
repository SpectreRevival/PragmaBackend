#pragma once

#include <PacketProcessor.h>

class GetHostConnectionDetailsProcessor : public WebsocketPacketProcessor {
  public:
    explicit GetHostConnectionDetailsProcessor(const SpectreRpcType& rpcType);
    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};
