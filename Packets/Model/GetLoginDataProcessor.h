#pragma once
#include <PacketProcessor.h>

class GetLoginDataProcessor : public WebsocketPacketProcessor {
  public:
    explicit GetLoginDataProcessor(const SpectreRpcType& rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};