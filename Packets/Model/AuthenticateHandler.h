#pragma once
#include <PacketProcessor.h>

class AuthenticateHandler : public HTTPPacketProcessor {
  public:
    explicit AuthenticateHandler(HTTPRequestIdentifier id);
    std::optional<drogon::HttpResponsePtr> Process(const drogon::HttpRequestPtr& req) override;

  private:
    static std::string CreatePlayerFromSteam(const std::string& steam64, const std::string& displayName);
    static std::string BuildJwt(
        const std::string& backendType,
        const std::string& playerId,
        const std::string& socialId,
        const std::string& displayName,
        const std::string& discriminator);
};