#include "spdlog/spdlog.h"

#include <HTTPRequestIdentifier.h>

HTTPRequestIdentifier::HTTPRequestIdentifier(std::string route, HTTPRequestType reqType)
    : route(route), reqType(reqType){

}

HTTPRequestIdentifier::HTTPRequestIdentifier(std::string route) : route(route), reqType(HTTPRequestType::GET) {
    spdlog::warn("No request type provided for route {}, assuming GET", route);
}

const HTTPRequestType& HTTPRequestIdentifier::GetRequestType() const {
    return reqType;
}

const std::string& HTTPRequestIdentifier::GetRoute() const {
    return route;
}

bool HTTPRequestIdentifier::operator==(const HTTPRequestIdentifier& other) const {
    return route == other.route && reqType == other.reqType;
}

