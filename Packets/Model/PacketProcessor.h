#pragma once
#include "restinio/request_handler.hpp"
#include "restinio/router/express.hpp"
#include "restinio/traits.hpp"

#include <HTTPRequestIdentifier.h>
#include <SpectreRpcType.h>
#include <SpectreWebsocketRequest.h>
#include <string>
#include <utility>
#include <WebsocketPayload.h>

class HTTPPacketProcessor {
  private:
    HTTPRequestIdentifier routeId;

  public:
    virtual ~HTTPPacketProcessor() = default;
    explicit HTTPPacketProcessor(HTTPRequestIdentifier routeId);
    explicit HTTPPacketProcessor(HTTPRequestIdentifier routeId, uint16_t port);
    HTTPPacketProcessor(HTTPPacketProcessor& other) = delete;
    HTTPPacketProcessor(HTTPPacketProcessor&& other) = delete;
    virtual std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> Process(restinio::request_handle_t req, restinio::router::route_params_t params) = 0;
    virtual restinio::request_handling_status_t ProcessResolveOptional(restinio::request_handle_t req, restinio::router::route_params_t params);
    [[nodiscard]] const std::string& GetRoute() const {
        return routeId.GetRoute();
    }
    [[nodiscard]] HTTPRequestType GetMethod() const {
        return routeId.GetRequestType();
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
    virtual std::optional<WebsocketPayload> Process(SpectreWebsocketRequest& packet) = 0;
    virtual ~WebsocketPacketProcessor() = default;
    [[nodiscard]] const SpectreRpcType& GetType() const {
        return rpcType;
    }
    static WebsocketPacketProcessor* GetProcessorForRpc(const SpectreRpcType& rpcType) {
        const auto it = websocketRoutes.find(rpcType);
        return it == websocketRoutes.end() ? nullptr : it->second;
    }
};