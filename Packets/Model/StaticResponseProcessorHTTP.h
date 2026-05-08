#pragma once

#include <PacketProcessor.h>

class StaticResponseProcessorHTTP : public HTTPPacketProcessor {
  private:
    std::string staticRes;

  public:
    StaticResponseProcessorHTTP(HTTPRequestIdentifier id, const nlohmann::json& res);

    std::optional<drogon::HttpResponsePtr> Process(const drogon::HttpRequestPtr& req) override;
};