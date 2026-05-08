#include "spdlog/spdlog.h"

#include <Notification.h>
#include <google/protobuf/util/json_util.h>
#include <utility>
#include <uuid.h>

static google::protobuf::util::JsonPrintOptions opts = {
    .always_print_fields_with_no_presence = true};

static std::mt19937 rng = std::mt19937{std::random_device{}()};
static uuids::uuid_random_generator gen{rng};

Notification::Notification(const SpectreRpcType& notificationType, const google::protobuf::Message& notificationData)
    : notificationType(notificationType), notificationId(uuids::to_string(gen())) {
    if (!google::protobuf::util::MessageToJsonString(notificationData, &(this->notificationData), opts).ok()) {
        spdlog::error("Failed to turn protobuf notification data into json string");
    }
}

Notification::Notification(const SpectreRpcType& notificationType, std::string notificationId, std::string notificationPayload)
    : notificationType(notificationType), notificationId(std::move(notificationId)), notificationData(std::move(notificationPayload)) {
}

const SpectreRpcType& Notification::GetNotificationType() const {
    return notificationType;
}

const std::string& Notification::GetNotificationData() const {
    return notificationData;
}

const std::string& Notification::GetNotificationId() const {
    return notificationId;
}