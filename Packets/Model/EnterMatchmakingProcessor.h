#pragma once
#include <PacketProcessor.h>

class EnterMatchmakingProcessor : public WebsocketPacketProcessor {
  public:
    explicit EnterMatchmakingProcessor(const SpectreRpcType& rpcType);
    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};