#pragma once
#include <PacketProcessor.h>

class GetFriendsListAndRegisterOnlineHandler : public WebsocketPacketProcessor {
  public:
    explicit GetFriendsListAndRegisterOnlineHandler(SpectreRpcType rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};