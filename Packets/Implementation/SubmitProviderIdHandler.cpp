#include "AuthLatch.h"
#include "AuthenticateHandler.h"
#include "ResourcesUtilities.h"

#include <SteamValidator.h>
#include <SubmitProviderIdHandler.h>
#include <fstream>
#include <utility>

SubmitProviderIdHandler::SubmitProviderIdHandler(HTTPRequestIdentifier id)
    : HTTPPacketProcessor(std::move(id)) {
}

static std::string GetSteamApiKey() {
    std::ifstream authFile(ResourcesUtilities::GetResourcesFolder() / "../" / "auth.json");
    std::stringstream ss;
    ss << authFile.rdbuf();
    nlohmann::json authJson = nlohmann::json::parse(ss.str());
    return authJson.at("steamApiKey").get<std::string>();
}

std::optional<drogon::HttpResponsePtr> SubmitProviderIdHandler::Process(const drogon::HttpRequestPtr& req) {
    if (GetSteamApiKey().empty()) {
        auto res = HttpResponse::newHttpResponse();
        res->setBody("no steam api key provided");
        return res;
    }

    const auto body = nlohmann::json::parse(req->body(), nullptr, false);
    if (body.is_discarded() || !body.contains("providerId") || !body.at("providerId").is_string()) {
        auto res = HttpResponse::newHttpResponse();
        res->setBody("err: provider id required");
        return res;
    }
    const std::string steam64 = body.at("providerId");
    if (steam64.empty()) {
        auto res = HttpResponse::newHttpResponse();
        res->setBody("err: steam id required");
        return res;
    }

    SteamValidator v(GetSteamApiKey());
    auto info = v.ValidateSteamId(steam64);
    if (!info) {
        auto res = HttpResponse::newHttpResponse();
        res->setBody("err: invalid steam id");
        return res;
    }

    AuthLatch::Get().Put(req->peerAddr().toIp(), steam64, /*latch timer in seconds*/ 120);
    // 120s for now until i sort the launcher out - astro
    auto res = HttpResponse::newHttpResponse();
    res->setBody(R"({"ok":true})");
    return res;
}