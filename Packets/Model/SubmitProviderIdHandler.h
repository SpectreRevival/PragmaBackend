#pragma once
#include <PacketProcessor.h>

class SubmitProviderIdHandler : public HTTPPacketProcessor {
  public:
    explicit SubmitProviderIdHandler(HTTPRequestIdentifier id);

    std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> Process(restinio::request_handle_t req, restinio::router::route_params_t params) override;
};