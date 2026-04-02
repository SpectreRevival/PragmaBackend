#pragma once
#include <PacketProcessor.h>

class GetBulkProfileDataProcessor : public WebsocketPacketProcessor {
  public:
    explicit GetBulkProfileDataProcessor(const SpectreRpcType& rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};