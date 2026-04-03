#pragma once
#include <SpectreRpcType.h>
#include <google/protobuf/message.h>

class Notification {
  private:
    SpectreRpcType notificationType;
    const std::string notificationId;
    std::string notificationData;

  public:
    Notification(const SpectreRpcType& notificationType, const google::protobuf::Message& notificationData);
    Notification(const SpectreRpcType& notificationType, const std::string& notificationId, const std::string& notificationData);
    const SpectreRpcType& GetNotificationType() const;
    const std::string& GetNotificationId() const;
    const std::string& GetNotificationData() const;
};