#pragma once
#include "Regex.h"

#include <PacketProcessor.h>
#include <nlohmann/json.hpp>
#include <string>
#include <unordered_map>

class RegexPayloadProcessorWS : public WebsocketPacketProcessor {
  private:
    std::unordered_map<Regex, std::shared_ptr<nlohmann::json>> resMap;

  public:
    RegexPayloadProcessorWS(const SpectreRpcType& rpcType, const std::unordered_map<Regex, std::shared_ptr<nlohmann::json>>& resMap)
        : WebsocketPacketProcessor(rpcType), resMap(resMap){};

    std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) override;
};