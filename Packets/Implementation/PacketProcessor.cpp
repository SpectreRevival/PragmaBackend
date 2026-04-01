#include <PacketProcessor.h>
#include <RequestRouter.h>

HTTPPacketProcessor::HTTPPacketProcessor(HTTPRequestIdentifier routeId) :
    routeId(routeId) {
    RequestRouter::RegisterHTTPProcessor(this);
}

HTTPPacketProcessor::HTTPPacketProcessor(HTTPRequestIdentifier routeId, uint16_t port) :
routeId(routeId) {
    RequestRouter::RegisterHTTPProcessor(port, this);
}

restinio::request_handling_status_t HTTPPacketProcessor::ProcessResolveOptional(restinio::request_handle_t req, restinio::router::route_params_t params) {
    std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> response = Process(req, std::move(params));
    if (!response.has_value()) {
        return restinio::request_handling_status_t::not_handled;
    }
    return response.value().done();
}