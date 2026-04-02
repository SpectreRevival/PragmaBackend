#pragma once
#include <PacketProcessor.h>

class UpdatePartyPlayerProcessor : public WebsocketPacketProcessor {
  public:
    explicit UpdatePartyPlayerProcessor(SpectreRpcType rpcType);
    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};