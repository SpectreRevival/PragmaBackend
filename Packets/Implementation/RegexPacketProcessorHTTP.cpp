#include <RegexPayloadProcessorHTTP.h>
#include <spdlog/spdlog.h>

std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> RegexPayloadProcessorHTTP::Process(restinio::request_handle_t req, restinio::router::route_params_t params) {
    for (const auto& [regex, payload] : resMap) {
        if (std::regex_search(req->body(), regex.rx)) {
            return req->create_response().set_body(payload->dump());
        }
    }
    return std::nullopt;
    spdlog::error("Regex processor {} failed to find any valid response for the packet, dropping the packet.\nPacket contents: {}", GetRoute(), req->body());
}