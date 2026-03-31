#pragma once
#include <PacketProcessor.h>

class AuthenticateHandler : public HTTPPacketProcessor {
  public:
    explicit AuthenticateHandler(HTTPRequestIdentifier id);
    std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> Process(restinio::request_handle_t req, restinio::router::route_params_t params) override;

  private:
    static std::string CreatePlayerFromSteam(const std::string& steam64, const std::string& displayName);
    static std::string BuildJwt(
        const std::string& backendType,
        const std::string& playerId,
        const std::string& socialId,
        const std::string& displayName,
        const std::string& discriminator);
};