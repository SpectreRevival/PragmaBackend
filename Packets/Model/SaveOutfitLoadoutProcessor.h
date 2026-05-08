#pragma once
#include <PacketProcessor.h>

class SaveOutfitLoadoutProcessor : public WebsocketPacketProcessor {
  public:
    explicit SaveOutfitLoadoutProcessor(SpectreRpcType rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};