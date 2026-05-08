#pragma once
#include "Notification.h"

#include <google/protobuf/message.h>
#include <nlohmann/json.hpp>
#include <string>

class WebsocketPayload {
  private:
    std::string payload;
    std::vector<Notification> postSendNotifications{};

  public:
    WebsocketPayload(const google::protobuf::Message& message);
    WebsocketPayload(std::string payload);
    WebsocketPayload(const nlohmann::json& json);
    WebsocketPayload(const google::protobuf::Message& message, std::initializer_list<Notification> postSendNotifications);
    WebsocketPayload(const nlohmann::json& json, std::initializer_list<Notification> postSendNotifications);
    WebsocketPayload(std::string payload, std::initializer_list<Notification> postSendNotifications);
    WebsocketPayload(const google::protobuf::Message& message, std::vector<Notification> postSendNotifications);
    WebsocketPayload(const nlohmann::json& json, std::vector<Notification> postSendNotifications);
    WebsocketPayload(std::string message, std::vector<Notification> postSendNotifications);
    [[nodiscard]] const std::string& GetPayload() const;
    [[nodiscard]] const std::vector<Notification>& GetPostSendNotifications() const;
};