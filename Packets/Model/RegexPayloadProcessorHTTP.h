#pragma once
#include "Regex.h"

#include <PacketProcessor.h>
#include <unordered_map>

class RegexPayloadProcessorHTTP : public HTTPPacketProcessor {
  private:
    std::unordered_map<Regex, std::shared_ptr<json>> resMap;

  public:
    RegexPayloadProcessorHTTP(HTTPRequestIdentifier id, const std::unordered_map<Regex, std::shared_ptr<json>>& resMap)
        : HTTPPacketProcessor(id), resMap(resMap){};

    std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> Process(restinio::request_handle_t req, restinio::router::route_params_t params) override;
};