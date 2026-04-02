#pragma once
#include <PacketProcessor.h>

class UpdateItemV4Processor : public WebsocketPacketProcessor {
  public:
    explicit UpdateItemV4Processor(SpectreRpcType rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};