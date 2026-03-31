#pragma once
#include "restinio/request_handler.hpp"
#include "restinio/router/express.hpp"
#include "restinio/traits.hpp"

#include <HTTPRequestIdentifier.h>
#include <SpectreRpcType.h>
#include <SpectreWebsocketRequest.h>
#include <string>
#include <utility>

class HTTPPacketProcessor {
  private:
    HTTPRequestIdentifier routeId;
    inline static std::unordered_map<HTTPRequestIdentifier, HTTPPacketProcessor*> httpRoutes = {};

  public:
    explicit HTTPPacketProcessor(HTTPRequestIdentifier routeId)
        : routeId(std::move(routeId)) {
        httpRoutes.insert_or_assign(routeId, this);
    };
    HTTPPacketProcessor(HTTPPacketProcessor& other) = delete;
    HTTPPacketProcessor(HTTPPacketProcessor&& other) = delete;
    virtual std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> Process(restinio::request_handle_t req, restinio::router::route_params_t params) = 0;
    virtual ~HTTPPacketProcessor() {
        httpRoutes.erase(routeId);
    }
    [[nodiscard]] const std::string& GetRoute() const {
        return routeId.GetRoute();
    }
    static HTTPPacketProcessor* GetProcessorForRoute(const HTTPRequestIdentifier& routeId) {
        auto it = httpRoutes.find(routeId);
        return it == httpRoutes.end() ? nullptr : it->second;
    }
};

class WebsocketPacketProcessor {
  private:
    SpectreRpcType rpcType;
    inline static std::unordered_map<SpectreRpcType, WebsocketPacketProcessor*> websocketRoutes = {};

  public:
    explicit WebsocketPacketProcessor(const SpectreRpcType& rpcType)
        : rpcType(rpcType) {
        websocketRoutes[rpcType] = this;
    }
    virtual void Process(SpectreWebsocketRequest& packet, SpectreWebsocket& sock) = 0;
    virtual ~WebsocketPacketProcessor() = default;
    [[nodiscard]] const SpectreRpcType& GetType() const {
        return rpcType;
    }
    static WebsocketPacketProcessor* GetProcessorForRpc(const SpectreRpcType& rpcType) {
        const auto it = websocketRoutes.find(rpcType);
        return it == websocketRoutes.end() ? nullptr : it->second;
    }
};