#include <RequestRouter.h>
#include <restinio/websocket/websocket.hpp>
#include <restinio/all.hpp>

std::vector<restinio::running_server_handle_t<RestinioServerTraits>> RequestRouter::servers{};
std::vector<rws::ws_handle_t> RequestRouter::websocketConnections{};

static std::string StringWithoutQuery(std::string original) {
    auto queryPos = original.find('?');
    return queryPos == std::string::npos ? original : original.substr(0, queryPos);
}

static restinio::request_handling_status_t HTTPProcessor(restinio::request_handle_t req, restinio::router::route_params_t params, HTTPRequestType reqType, std::vector<rws::ws_handle_t>& wsConnections) {
    return req->create_response().set_body("test").done();
    if (req->header().connection() == restinio::http_connection_header_t::upgrade) {
        // upgrade connection to websocket
        rws::ws_handle_t websocketHandler = rws::upgrade<RestinioServerTraits>(*req, rws::activation_t::immediate,
            [](rws::ws_handle_t websocketHandler, rws::message_handle_t message) mutable {
                if( rws::opcode_t::text_frame == message->opcode() ||
                                rws::opcode_t::binary_frame == message->opcode() ||
                                rws::opcode_t::continuation_frame == message->opcode() )
                {

                    websocketHandler->send_message( *message );
                }
                // Continue responding to pings
                else if( rws::opcode_t::ping_frame == message->opcode() )
                {
                    auto resp = *message;
                    resp.set_opcode( rws::opcode_t::pong_frame );
                    websocketHandler->send_message( resp );
                }
            });
        wsConnections.push_back(websocketHandler);
        return restinio::request_accepted();
    }
    HTTPPacketProcessor* processor = HTTPPacketProcessor::GetProcessorForRoute(
            HTTPRequestIdentifier(req->header().request_target(), HTTPRequestType::GET)
            );
    if (processor == nullptr) {
        spdlog::error("Failed to find an HTTP processor for a request to {}, dropping", StringWithoutQuery(req->header().request_target()));
        return restinio::request_handling_status_t::not_handled;
    }
    std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> res = processor->Process(req, std::move(params));
    if (res.has_value()) {
        return res.value().done();
    }
    spdlog::debug("HTTP processor chose to not respond to a request");
    return restinio::request_handling_status_t::not_handled;
}

void RequestRouter::CreateServer(uint16_t port) {
    servers.push_back(restinio::run_async<RestinioServerTraits>(
        restinio::own_io_context(),
        restinio::server_settings_t<RestinioServerTraits>{}
            .port(port)
            .address("0.0.0.0")
            .request_handler(HTTPProcessor),
            2
            ));
}

void RequestRouter::Shutdown() {
    for (restinio::running_server_handle_t<RestinioServerTraits>& server : servers) {
        server->stop();
        server->wait();
    }
}