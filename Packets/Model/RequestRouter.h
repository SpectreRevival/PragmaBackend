#pragma once
#include <unordered_map>
#include <restinio/core.hpp>
#include <PacketProcessor.h>
#include <restinio/websocket/websocket.hpp>
#include <SpectreWebsocket.h>

namespace rws = restinio::websocket::basic;

struct RestinioServerTraits : public restinio::default_traits_t {
    using logger_t = restinio::null_logger_t;
    using timer_manager_t = restinio::asio_timer_manager_t;
    using request_handler_t = restinio::router::express_router_t<>;
};

class RequestRouter {
private:
    static std::unordered_map<uint16_t, std::unique_ptr<restinio::router::express_router_t<>>> routers;
    static std::vector<restinio::running_server_handle_t<RestinioServerTraits>> servers;
    static std::list<SpectreWebsocket> websocketConnections;
    static restinio::request_handling_status_t NonMatchedHTTPProcessor(restinio::request_handle_t req);
public:
    RequestRouter() = delete;
    static void CreateRouter(uint16_t port);
    static void RegisterHTTPProcessor(uint16_t port, HTTPPacketProcessor* processor);
    static void RegisterHTTPProcessor(HTTPPacketProcessor* processor);
    static void Start();
    static void Shutdown();
};