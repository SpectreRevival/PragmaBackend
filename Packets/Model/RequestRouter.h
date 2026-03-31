#pragma once
#include <unordered_map>
#include <restinio/core.hpp>
#include <PacketProcessor.h>

class RequestRouter {
private:
    static std::unordered_map<uint16_t, restinio::router::express_router_t<>*> routers;
    static std::vector<restinio::running_server_handle_t<RestinioServerTraits>> servers;
public:
    RequestRouter() = delete;
    static restinio::router::express_router_t<>* GetRouter(uint16_t port);
    static void CreateRouter(uint16_t port);
    static void Shutdown();
    static void Start();
};