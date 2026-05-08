#pragma once
#include <PacketProcessor.h>

class SubmitProviderIdHandler : public HTTPPacketProcessor {
  public:
    explicit SubmitProviderIdHandler(HTTPRequestIdentifier id);

    std::optional<drogon::HttpResponsePtr> Process(const drogon::HttpRequestPtr& req) override;
};