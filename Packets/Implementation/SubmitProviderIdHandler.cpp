#include "AuthLatch.h"
#include "AuthenticateHandler.h"
#include "ResourcesUtilities.h"

#include <SteamValidator.h>
#include <SubmitProviderIdHandler.h>
#include <fstream>

SubmitProviderIdHandler::SubmitProviderIdHandler(HTTPRequestIdentifier id)
    : HTTPPacketProcessor(id) {
}

static std::string GetSteamApiKey() {
    std::ifstream authFile(ResourcesUtilities::GetResourcesFolder() / "../" / "auth.json");
    std::stringstream ss;
    ss << authFile.rdbuf();
    json authJson = json::parse(ss.str());
    return authJson.at("steamApiKey").get<std::string>();
}

std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> SubmitProviderIdHandler::Process(restinio::request_handle_t req, restinio::router::route_params_t params) {
    if (GetSteamApiKey().empty()) {
        return req->create_response(restinio::status_bad_request()).set_body("no steam api key provided");
    }

    const auto body = json::parse(req->body(), nullptr, false);
    if (body.is_discarded() || !body.contains("providerId") || !body.at("providerId").is_string()) {
        return req->create_response(restinio::status_bad_request()).set_body("err: provider id required");
    }
    const std::string steam64 = body.at("providerId");
    if (steam64.empty()) {
        return req->create_response(restinio::status_bad_request()).set_body("err: steam id required");
    }

    SteamValidator v(GetSteamApiKey());
    auto info = v.ValidateSteamId(steam64);
    if (!info) {
        return req->create_response(restinio::status_bad_request()).set_body("err: invalid steam id");
    }

    AuthLatch::Get().Put(req->remote_endpoint().address().to_string(), steam64, /*latch timer in seconds*/ 120);
    // 120s for now until i sort the launcher out - astro
    return req->create_response().set_body(R"({"ok":true})");
}