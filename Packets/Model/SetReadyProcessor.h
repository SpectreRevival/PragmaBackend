#pragma once
#include <PacketProcessor.h>

class SetReadyProcessor : public WebsocketPacketProcessor {
  public:
    explicit SetReadyProcessor(SpectreRpcType rpcType);
    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};