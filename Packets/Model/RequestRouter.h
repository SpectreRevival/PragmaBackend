#pragma once
#include <unordered_map>
#include <restinio/core.hpp>
#include <PacketProcessor.h>
#include <restinio/websocket/websocket.hpp>

namespace rws = restinio::websocket::basic;

struct RestinioServerTraits : public restinio::default_traits_t {
    using logger_t = restinio::null_logger_t;
    using timer_manager_t = restinio::asio_timer_manager_t;
    using request_handler_t = restinio::router::express_router_t<>;
};

class RequestRouter {
private:
    static std::vector<restinio::running_server_handle_t<RestinioServerTraits>> servers;
    static std::vector<rws::ws_handle_t> websocketConnections;
public:
    RequestRouter() = delete;
    static void CreateServer(uint16_t port);
    static void Shutdown();
};