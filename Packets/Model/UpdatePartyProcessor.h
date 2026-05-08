#pragma once
#include <PacketProcessor.h>

class UpdatePartyProcessor : public WebsocketPacketProcessor {
  public:
    explicit UpdatePartyProcessor(SpectreRpcType rpcType);
    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};