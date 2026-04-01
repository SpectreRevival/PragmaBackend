#include <RequestRouter.h>
#include <restinio/websocket/websocket.hpp>
#include <restinio/all.hpp>

std::unordered_map<uint16_t, std::unique_ptr<restinio::router::express_router_t<>>> RequestRouter::routers{};
std::vector<restinio::running_server_handle_t<RestinioServerTraits>> RequestRouter::servers{};
std::vector<rws::ws_handle_t> RequestRouter::websocketConnections{};

static std::string StringWithoutQuery(std::string original) {
    auto queryPos = original.find('?');
    return queryPos == std::string::npos ? original : original.substr(0, queryPos);
}

static restinio::request_handling_status_t NonMatchedHTTPProcessor(restinio::request_handle_t req, std::vector<rws::ws_handle_t>& wsConnections) {
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
    spdlog::debug("No processor found for a request");
    return restinio::request_handling_status_t::not_handled;
}

void RequestRouter::CreateRouter(uint16_t port) {
    auto router = std::make_unique<restinio::router::express_router_t<>>();
    router->non_matched_request_handler([](auto req) {
        return NonMatchedHTTPProcessor(req, websocketConnections);
    });
    routers[port] = std::move(router);
}

void RequestRouter::RegisterHTTPProcessor(uint16_t port, HTTPPacketProcessor* processor) {
    if (!processor) {
        spdlog::warn("Tried to register route for null http packet processor, ignoring");
        return;
    }
    auto router = routers.find(port);
    if (router == routers.end()) {
        spdlog::error("Failed to find router for port {}, packet processor for route {} will not be registered", port, processor->GetRoute());
        return;
    }
    if (processor->GetMethod() == HTTPRequestType::GET) {
        router->second->http_get(processor->GetRoute(), std::bind(&HTTPPacketProcessor::ProcessResolveOptional, processor, std::placeholders::_1, std::placeholders::_2));
    }
    else if (processor->GetMethod() == HTTPRequestType::POST) {
        router->second->http_post(processor->GetRoute(), std::bind(&HTTPPacketProcessor::ProcessResolveOptional, processor, std::placeholders::_1, std::placeholders::_2));
    }
    else if (processor->GetMethod() == HTTPRequestType::PUT) {
        router->second->http_put(processor->GetRoute(), std::bind(&HTTPPacketProcessor::ProcessResolveOptional, processor, std::placeholders::_1, std::placeholders::_2));
    }
    else if (processor->GetMethod() == HTTPRequestType::DEL) {
        router->second->http_delete(processor->GetRoute(), std::bind(&HTTPPacketProcessor::ProcessResolveOptional, processor, std::placeholders::_1, std::placeholders::_2));
    }
    else if (processor->GetMethod() == HTTPRequestType::HEAD) {
        router->second->http_head(processor->GetRoute(), std::bind(&HTTPPacketProcessor::ProcessResolveOptional, processor, std::placeholders::_1, std::placeholders::_2));
    } else {
        spdlog::error("Failed to register HTTP processor for port {} and route {} due to an unrecognized HTTP method", port, processor->GetRoute());
    }
}

void RequestRouter::RegisterHTTPProcessor(HTTPPacketProcessor* processor) {
    for (auto& [port, router] : routers) {
        RegisterHTTPProcessor(port, processor);
    }
}

void RequestRouter::Start() {
    for (auto& [port, router] : routers) {
            servers.push_back(restinio::run_async<RestinioServerTraits>(
                restinio::own_io_context(),
                restinio::server_settings_t<RestinioServerTraits>{}
                    .port(port)
                    .address("0.0.0.0")
                    .request_handler(std::move(router)),
                    2
            ));
    }
}

void RequestRouter::Shutdown() {
    for (restinio::running_server_handle_t<RestinioServerTraits>& server : servers) {
        server->stop();
        server->wait();
    }
}