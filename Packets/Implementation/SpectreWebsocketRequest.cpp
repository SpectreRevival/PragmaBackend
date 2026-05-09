#include "SpectreWebsocketRequest.h"

#include <nlohmann/json.hpp>
#include <spdlog/spdlog.h>

SpectreWebsocketRequest::SpectreWebsocketRequest(std::string& reqBody, std::string& playerId)
    : reqBody(reqBody), playerId(playerId) {
    nlohmann::json reqjson = nlohmann::json::parse(reqBody);
    reqJson = std::make_shared<nlohmann::json>(reqjson);
    try {
        requestType = SpectreRpcType(std::string(reqJson->at("type")));
    } catch (std::exception& e) {
        spdlog::warn("log type not found for " + reqJson->at("type").get<std::string>());
    }
    requestId = reqJson->at("requestId");
    payloadAsStr = reqJson->at("payload").dump();
}

std::shared_ptr<nlohmann::json> SpectreWebsocketRequest::GetPayload() const {
    return std::make_shared<nlohmann::json>((reqJson->at("payload")));
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

int SpectreWebsocketRequest::GetRequestId() const {
    return requestId;
}

const std::string& SpectreWebsocketRequest::GetPlayerId() const {
    return playerId;
}