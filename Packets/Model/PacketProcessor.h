#pragma once
#include <HTTPRequestIdentifier.h>
#include <SpectreRpcType.h>
#include <SpectreWebsocketRequest.h>
#include <WebsocketPayload.h>
#include <drogon/HttpResponse.h>
#include <string>
#include <utility>

using namespace drogon;

class HTTPPacketProcessor {
  private:
    HTTPRequestIdentifier routeId;

  public:
    virtual ~HTTPPacketProcessor() = default;
    explicit HTTPPacketProcessor(HTTPRequestIdentifier routeId);
    explicit HTTPPacketProcessor(HTTPRequestIdentifier routeId, uint16_t port);
    HTTPPacketProcessor(HTTPPacketProcessor& other) = delete;
    HTTPPacketProcessor(HTTPPacketProcessor&& other) = delete;
    virtual std::optional<drogon::HttpResponsePtr> Process(const drogon::HttpRequestPtr& req) = 0;
    [[nodiscard]] const std::string& GetRoute() const {
        return routeId.GetRoute();
    }
    [[nodiscard]] drogon::HttpMethod GetMethod() const {
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