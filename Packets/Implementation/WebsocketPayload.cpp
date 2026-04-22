#include <WebsocketPayload.h>
#include <google/protobuf/util/json_util.h>
#include <spdlog/spdlog.h>
#include <utility>

const std::string& WebsocketPayload::GetPayload() const {
    return payload;
}

WebsocketPayload::WebsocketPayload(std::string payload)
    : WebsocketPayload(std::move(payload), {}) {
}

WebsocketPayload::WebsocketPayload(const nlohmann::json& json)
    : WebsocketPayload(json, {}) {
}

WebsocketPayload::WebsocketPayload(std::string payload, std::initializer_list<Notification> postSendNotifications)
    : payload(std::move(payload)), postSendNotifications(postSendNotifications) {
}

WebsocketPayload::WebsocketPayload(const nlohmann::json& json, std::initializer_list<Notification> postSendNotifications)
    : payload(json.dump()), postSendNotifications(postSendNotifications) {
}

WebsocketPayload::WebsocketPayload(const google::protobuf::Message& message, std::initializer_list<Notification> postSendNotifications)
    : postSendNotifications(postSendNotifications) {
    static google::protobuf::util::JsonPrintOptions opts{};
    opts.always_print_fields_with_no_presence = true;
    if (!google::protobuf::util::MessageToJsonString(message, &payload, opts).ok()) {
        spdlog::error("Failed to convert protobuf message to websocket payload");
        payload = "";
    }
}

WebsocketPayload::WebsocketPayload(std::string message, std::vector<Notification> postSendNotifications)
    : payload(std::move(std::move(message))), postSendNotifications(std::move(std::move(postSendNotifications))) {
}

WebsocketPayload::WebsocketPayload(const google::protobuf::Message& message, std::vector<Notification> postSendNotifications)
    : postSendNotifications(std::move(std::move(postSendNotifications))) {
    static google::protobuf::util::JsonPrintOptions opts{};
    opts.always_print_fields_with_no_presence = true;
    if (!google::protobuf::util::MessageToJsonString(message, &payload, opts).ok()) {
        spdlog::error("Failed to convert protobuf message to websocket payload");
        payload = "";
    }
}

WebsocketPayload::WebsocketPayload(const nlohmann::json& json, std::vector<Notification> postSendNotifications)
    : payload(json.dump()), postSendNotifications(std::move(std::move(postSendNotifications))) {
}

WebsocketPayload::WebsocketPayload(const google::protobuf::Message& message)
    : WebsocketPayload(message, {}) {}

const std::vector<Notification>& WebsocketPayload::GetPostSendNotifications() const {
    return postSendNotifications;
}