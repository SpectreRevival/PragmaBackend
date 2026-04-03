#include <SpectreWebsocket.h>
#include <google/protobuf/util/json_util.h>
#include <jwt-cpp/jwt.h>
#include <jwt-cpp/traits/nlohmann-json/traits.h>
#include <nlohmann/json.hpp>
#include <spdlog/spdlog.h>
#include <SpectreWebsocketRequest.h>
#include <PacketProcessor.h>

static pbuf::util::JsonPrintOptions opts = []() {
    static pbuf::util::JsonPrintOptions options;
    options.always_print_fields_with_no_presence = true;
    return options;
}();

static std::string ExtractBearer(const std::string& authField) {
    if (authField.empty()) return "";
    static constexpr std::string_view prefix = "Bearer ";
    const std::string s = std::string(authField);
    if (!s.starts_with(prefix)) return {};
    return s.substr(prefix.size());
}

static std::string DecodePlayerIdNoverify(const std::string& token) {
    try {
        const auto decoded = jwt::decode<jwt::traits::nlohmann_json>(token);

        if (decoded.has_payload_claim("pragmaPlayerId")) {
            return decoded.get_payload_claim("pragmaPlayerId").as_string();
        }

        try {
            return decoded.get_subject();
        } catch (...) {
            if (decoded.has_payload_claim("sub")) {
                return decoded.get_payload_claim("sub").as_string();
            }
        }
    } catch (const std::exception& e) {
        spdlog::warn("JWT decode failed: {}", e.what());
    }
    return {};
}

SpectreWebsocket::SpectreWebsocket(restinio::request_handle_t initialRequest)
    : curSequenceNumber(0) {
    const auto bearer = ExtractBearer(initialRequest->header().get_field_or("Authorization", ""));
    const auto pid = bearer.empty() ? std::string() : DecodePlayerIdNoverify(bearer);

    if (!pid.empty()) {
        playerId = pid;
    } else {
        spdlog::error("no playerid ???? investigate me!");
        playerId = "1";
    }
};

void SpectreWebsocket::OnReceiveWebsocketMessage(rws::ws_handle_t websocketHandler, rws::message_handle_t message) {
    SpectreWebsocketRequest request(message->payload(), playerId);
    WebsocketPacketProcessor* processor = WebsocketPacketProcessor::GetProcessorForRpc(request.GetRequestType());
    if (processor == nullptr) {
        spdlog::warn("Failed to find websocket message for rpc type {}", request.GetRequestType().GetName());
        return;
    }
    std::optional<WebsocketPayload> response = processor->Process(request);
    if (!response.has_value()) {
        return;
    }
    std::string finalResponse = FormulateFinalResponse(response.value().GetPayload(), request.GetRequestId(), request.GetResponseType());
    websocketHandler->send_message(rws::final_frame_flag_t::final_frame, rws::opcode_t::text_frame, finalResponse);
}

std::string SpectreWebsocket::FormulateFinalResponse(const std::shared_ptr<json>& res) {
    json packet;
    packet["sequenceNumber"] = curSequenceNumber;
    packet["response"] = *res;
    curSequenceNumber++;
    // dont pass temporary because beast will fragment it
    std::string msg = packet.dump();
    return msg;
}

std::string SpectreWebsocket::FormulateFinalResponse(const pbuf::Message& payload, const std::string& resType, int requestId) {
    // you shan't comment on this cursedness
    std::string finalRes = "{\"sequenceNumber\":" + std::to_string(curSequenceNumber) + R"(,"response":{"requestId":)" + std::to_string(requestId) + R"(,"type":")" + resType + R"(","payload":)";
    std::string resComponent;
    if (!pbuf::util::MessageToJsonString(payload, &resComponent, opts).ok()) {
        spdlog::error("Failed to serialize pbuf message to string in SendPacket");
        throw;
    }
    finalRes += resComponent + "}}";
    curSequenceNumber++;
    return finalRes;
}

std::string SpectreWebsocket::FormulateFinalResponse(const std::string& resPayload, int requestId, const std::string& resType) {
    std::string finalRes = "{\"sequenceNumber\":" + std::to_string(curSequenceNumber) + R"(,"response":{"requestId":)" + std::to_string(requestId) + R"(,"type":")" + resType + R"(","payload":)";
    finalRes += resPayload + "}}";
    curSequenceNumber++;
    return finalRes;
}

const std::string& SpectreWebsocket::GetPlayerId() {
    return playerId;
}