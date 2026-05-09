#pragma once
#include "Regex.h"

#include <PacketProcessor.h>
#include <unordered_map>

class RegexPayloadProcessorHTTP : public HTTPPacketProcessor {
  private:
    std::unordered_map<Regex, std::shared_ptr<nlohmann::json>> resMap;

  public:
    RegexPayloadProcessorHTTP(HTTPRequestIdentifier id, const std::unordered_map<Regex, std::shared_ptr<nlohmann::json>>& resMap)
        : HTTPPacketProcessor(id), resMap(resMap) {};

    std::optional<drogon::HttpResponsePtr> Process(const drogon::HttpRequestPtr& req) override;
};