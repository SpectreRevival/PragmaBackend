#pragma once
#include <PacketProcessor.h>

class SavePlayerDataProcessor : public WebsocketPacketProcessor {
  public:
    explicit SavePlayerDataProcessor(SpectreRpcType rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};