#include <RegexPayloadProcessorWS.h>

std::optional<WebsocketPayload> RegexPayloadProcessorWS::Process(SpectreWebsocketRequest& packet) {
    nlohmann::json res{};
    std::string payloadStr = packet.GetPayload()->dump();
    for (const auto& [regex, payload] : resMap) {
        if (std::regex_search(payloadStr, regex.rx)) {
            res = *payload;
            return WebsocketPayload(res);
        }
    }
    spdlog::warn("Failed to find something to send for regex processor, sending empty packet");
    return nlohmann::json::object();
}