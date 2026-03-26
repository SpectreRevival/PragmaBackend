#include "SpectreWebsocketRequest.h"

#include <nlohmann/json.hpp>
#include <spdlog/spdlog.h>

SpectreWebsocketRequest::SpectreWebsocketRequest(SpectreWebsocket& sock, std::string& reqBody)
    : websocket(sock), reqBody(reqBody) {
    json reqjson = json::parse(reqBody);
    reqJson = std::make_shared<json>(reqjson);
    try {
        requestType = SpectreRpcType(std::string(reqJson->at("type")));
    } catch (std::exception& e) {
        spdlog::warn("log type not found for " + reqJson->at("type").get<std::string>());
    }
    requestId = reqJson->at("requestId");
    payloadAsStr = reqJson->at("payload").dump();
}

std::shared_ptr<json> SpectreWebsocketRequest::GetPayload() const {
    return std::make_shared<json>((reqJson->at("payload")));
}

std::shared_ptr<json> SpectreWebsocketRequest::GetBaseJsonResponse() {
    json response;
    response["requestId"] = requestId;
    response["type"] = GetResponseType();
    response["payload"] = json::object();
    return std::make_shared<json>(std::move(response));
}

void SpectreWebsocketRequest::SendEmptyResponse() {
    websocket.SendPacket(GetBaseJsonResponse());
}

std::string SpectreWebsocketRequest::GetResponseType() const {
    std::string resType = requestType.GetName();
    if (resType.size() >= 7 && resType.ends_with("Request")) {
        resType.erase(resType.size() - 7);
    }
    resType += "Response";
    return resType;
}

const std::string& SpectreWebsocketRequest::GetBody() const {
    return reqBody;
}

SpectreRpcType SpectreWebsocketRequest::GetRequestType() const {
    return requestType;
}

SpectreWebsocket& SpectreWebsocketRequest::GetSocket() const {
    return websocket;
}

int SpectreWebsocketRequest::GetRequestId() const {
    return requestId;
}