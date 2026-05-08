#pragma once
#include <PacketProcessor.h>
#include <PlayerData.pb.h>

class GetPlayerDataProcessor : public WebsocketPacketProcessor {
  public:
    explicit GetPlayerDataProcessor(const SpectreRpcType& rpcType);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
    static std::string GetPlayerDataAsString(const PlayerData& playerData);
};