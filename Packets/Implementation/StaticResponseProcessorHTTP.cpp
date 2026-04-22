#include <StaticResponseProcessorHTTP.h>
#include <utility>

StaticResponseProcessorHTTP::StaticResponseProcessorHTTP(HTTPRequestIdentifier id, const nlohmann::json& res)
    : HTTPPacketProcessor(std::move(id)), staticRes(res.dump()) {
}

std::optional<drogon::HttpResponsePtr> StaticResponseProcessorHTTP::Process(const drogon::HttpRequestPtr& /*req*/) {
    auto res = HttpResponse::newHttpResponse();
    res->setBody(staticRes);
    return res;
}