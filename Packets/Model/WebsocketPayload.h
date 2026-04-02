#pragma once
#include <google/protobuf/message.h>
#include <string>
#include <nlohmann/json.hpp>

class WebsocketPayload {
private:
    std::string payload;
    public:
    explicit WebsocketPayload(const google::protobuf::Message& message);
    explicit WebsocketPayload(const std::string& payload);
    explicit WebsocketPayload(const nlohmann::json& json);
    [[nodiscard]] const std::string& GetPayload() const;
};