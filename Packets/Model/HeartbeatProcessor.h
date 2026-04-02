#pragma once
#include <PacketProcessor.h>

class HeartbeatProcessor : public WebsocketPacketProcessor {
  public:
    explicit HeartbeatProcessor(const SpectreRpcType& rpcType)
        : WebsocketPacketProcessor(rpcType){};
    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override {
        packet.SendEmptyResponse();
    }
};