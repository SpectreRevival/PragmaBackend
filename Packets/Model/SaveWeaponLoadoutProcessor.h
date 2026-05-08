#pragma once
#include <PacketProcessor.h>

class SaveWeaponLoadoutProcessor : public WebsocketPacketProcessor {
  public:
    explicit SaveWeaponLoadoutProcessor(SpectreRpcType rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};