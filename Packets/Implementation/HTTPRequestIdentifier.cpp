#include "spdlog/spdlog.h"

#include <HTTPRequestIdentifier.h>
#include <utility>

HTTPRequestIdentifier::HTTPRequestIdentifier(std::string route, drogon::HttpMethod reqType)
    : route(std::move(std::move(route))), reqType(reqType) {
}

HTTPRequestIdentifier::HTTPRequestIdentifier(std::string route)
    : route(route), reqType(drogon::Get) {
    spdlog::warn("No request type provided for route {}, assuming GET", route);
}

const drogon::HttpMethod& HTTPRequestIdentifier::GetRequestType() const {
    return reqType;
}

const std::string& HTTPRequestIdentifier::GetRoute() const {
    return route;
}

bool HTTPRequestIdentifier::operator==(const HTTPRequestIdentifier& other) const {
    return route == other.route && reqType == other.reqType;
}
