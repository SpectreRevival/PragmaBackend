#include <WebsocketPayload.h>
#include <google/protobuf/util/json_util.h>
#include <spdlog/spdlog.h>

const std::string& WebsocketPayload::GetPayload() const {
    return payload;
}

WebsocketPayload::WebsocketPayload(const std::string& payload) : payload(payload) {

}

WebsocketPayload::WebsocketPayload(const nlohmann::json& json) : payload(json.dump()) {

}

WebsocketPayload::WebsocketPayload(const google::protobuf::Message& message) {
    static google::protobuf::util::JsonPrintOptions opts{};
    opts.always_print_fields_with_no_presence = true;
    if (!google::protobuf::util::MessageToJsonString(message, &payload, opts).ok()) {
        spdlog::error("Failed to convert protobuf message to websocket payload");
        payload = "";
    }
}