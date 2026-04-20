#include <RegexPayloadProcessorHTTP.h>
#include <spdlog/spdlog.h>

    std::optional<drogon::HttpResponsePtr> RegexPayloadProcessorHTTP::Process(const drogon::HttpRequestPtr& req){
    for (const auto& [regex, payload] : resMap) {
        if (std::regex_search(req->body().begin(), req->body().end(), regex.rx)) {
            auto res = HttpResponse::newHttpResponse();
            res->setBody(payload->dump());
            return res;
        }
    }
    return std::nullopt;
    spdlog::error("Regex processor {} failed to find any valid response for the packet, dropping the packet.\nPacket contents: {}", GetRoute(), req->body());
}