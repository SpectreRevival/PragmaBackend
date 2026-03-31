#include <RequestRouter.h>

std::unordered_map<uint16_t, restinio::router::express_router_t<>*> RequestRouter::routers{};
std::vector<restinio::running_server_handle_t<RestinioServerTraits>> RequestRouter::servers{};

static std::string StringWithoutQuery(std::string original) {
    auto queryPos = original.find('?');
    return queryPos == std::string::npos ? original : original.substr(0, queryPos);
}

static restinio::request_handling_status_t HTTPProcessor(restinio::request_handle_t req, restinio::router::route_params_t params, HTTPRequestType reqType) {
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
    return restinio::request_handling_status_t::not_handled;
}

void RequestRouter::CreateRouter(uint16_t port) {
    auto router = new restinio::router::express_router_t<>();
    router->http_get(R"(/.*)", [](restinio::request_handle_t req, restinio::router::route_params_t params) {
        return HTTPProcessor(req, std::move(params), HTTPRequestType::GET);
    });
    router->http_post(R"(/.*)", [](restinio::request_handle_t req, restinio::router::route_params_t params) {
        return HTTPProcessor(req, std::move(params), HTTPRequestType::POST);
    });
    router->http_put(R"(/.*)", [](restinio::request_handle_t req, restinio::router::route_params_t params) {
        return HTTPProcessor(req, std::move(params), HTTPRequestType::PUT);
    });
    router->http_head(R"(/.*)", [](restinio::request_handle_t req, restinio::router::route_params_t params) {
        return HTTPProcessor(req, std::move(params), HTTPRequestType::HEAD);
    });
    router->http_delete(R"(/.*)", [](restinio::request_handle_t req, restinio::router::route_params_t params) {
        return HTTPProcessor(req, std::move(params), HTTPRequestType::DEL);
    });
    routers.insert_or_assign(port, router);
}

restinio::router::express_router_t<>* RequestRouter::GetRouter(uint16_t port) {
    auto it = routers.find(port);
    if (it == routers.end()) {
        return nullptr;
    }
    return it->second;
}

void RequestRouter::Shutdown() {
    for (restinio::running_server_handle_t<RestinioServerTraits>& server : servers) {
        server->stop();
        server->wait();
    }
    std::ranges::for_each(routers, [](std::pair<uint16_t, restinio::router::express_router_t<>*> kv) {
        delete kv.second;
    });
}

void RequestRouter::Start() {
    std::ranges::for_each(routers, [](std::pair<uint16_t, restinio::router::express_router_t<>*> kv) {
        servers.push_back(restinio::run_async(
            restinio::own_io_context(),
            restinio::server_settings_t<RestinioServerTraits>{},
            4
            ));
    });
}