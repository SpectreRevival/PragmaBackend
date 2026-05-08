#pragma once
#include <PacketProcessor.h>
#include <nlohmann/json.hpp>
#include <spdlog/spdlog.h>

class StaticResponseProcessorWS : public WebsocketPacketProcessor {
  private:
    std::string staticRes;

  public:
    StaticResponseProcessorWS(const SpectreRpcType rpcType, const nlohmann::json& res);

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};