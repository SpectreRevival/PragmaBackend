#pragma once
#include <PacketProcessor.h>

class UpdateItemsV0Processor : public WebsocketPacketProcessor {
  public:
    explicit UpdateItemsV0Processor(SpectreRpcType rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};