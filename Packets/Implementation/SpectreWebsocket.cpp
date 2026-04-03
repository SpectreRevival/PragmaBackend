#include "PlayerDatabase.h"
#include "SavedNotificationData.pb.h"

#include <PacketProcessor.h>
#include <SpectreWebsocket.h>
#include <SpectreWebsocketRequest.h>
#include <google/protobuf/util/json_util.h>
#include <jwt-cpp/jwt.h>
#include <jwt-cpp/traits/nlohmann-json/traits.h>
#include <nlohmann/json.hpp>
#include <spdlog/spdlog.h>

std::unordered_map<std::string, SpectreWebsocket*> SpectreWebsocket::connectionsByPlayerId{};
std::mutex SpectreWebsocket::connectionsMapMutex{};

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
        spdlog::error("no playerid ???? investigate me!, thinks will be SEVERELY wrong for connection with ip {} port {}", initialRequest->remote_endpoint().address().to_string(), initialRequest->remote_endpoint().port());
        playerId = "1";
    }
    std::unique_ptr<SavedNotificationData> notificationsFromDisk = PlayerDatabase::Get().GetField<SavedNotificationData>(FieldKey::NOTIFICATION_DATA, playerId);
    for (int i = 0; i < notificationsFromDisk->notificationstodeliver_size(); i++) {
        const SavedNotification& currentNotif = notificationsFromDisk->notificationstodeliver(i);
        notificationsToDeliver.push(Notification(SpectreRpcType(currentNotif.rpctype()), currentNotif.notificationid(), currentNotif.notificationdata()));
    }
    notificationWorkerThread = std::jthread([this](std::stop_token st) {
        NotificationThread(st);
    });
    std::unique_lock connectionsMapLock(connectionsMapMutex);
    connectionsByPlayerId.insert_or_assign(playerId, this);
}

void SpectreWebsocket::NotificationThread(std::stop_token st) {
    while (!st.stop_requested()) {
        if (!notificationQueueLock.try_lock()) {
            continue;
        }
        if (notificationsToDeliver.empty()) {
            notificationQueueLock.unlock();
            continue;
        }
        Notification& notification = notificationsToDeliver.front();
        websocketHandle->send_message(rws::final_frame_flag_t::final_frame, rws::opcode_t::text_frame, FormulateFinalNotification(notification));
        notificationsToDeliver.pop();
        SavedNotificationData savedData;
        for (const Notification& notif : notificationsToDeliver._Get_container()) {
            SavedNotification* newNotif = savedData.add_notificationstodeliver();
            newNotif->set_notificationdata(notif.GetNotificationData());
            newNotif->set_notificationid(notif.GetNotificationId());
            newNotif->set_rpctype(notif.GetNotificationType().GetName());
        }
        PlayerDatabase::Get().SetField(FieldKey::NOTIFICATION_DATA, &savedData, playerId);
        notificationQueueLock.unlock();
    }
}

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
    packet["sequenceNumber"] = curSequenceNumber.fetch_add(1);
    packet["response"] = *res;
    // dont pass temporary because beast will fragment it
    std::string msg = packet.dump();
    return msg;
}

std::string SpectreWebsocket::FormulateFinalResponse(const pbuf::Message& payload, const std::string& resType, int requestId) {
    // you shan't comment on this cursedness
    std::string finalRes = "{\"sequenceNumber\":" + std::to_string(curSequenceNumber.fetch_add(1)) + R"(,"response":{"requestId":)" + std::to_string(requestId) + R"(,"type":")" + resType + R"(","payload":)";
    std::string resComponent;
    if (!pbuf::util::MessageToJsonString(payload, &resComponent, opts).ok()) {
        spdlog::error("Failed to serialize pbuf message to string in SendPacket");
        throw;
    }
    finalRes += resComponent + "}}";
    return finalRes;
}

std::string SpectreWebsocket::FormulateFinalResponse(const std::string& resPayload, int requestId, const std::string& resType) {
    std::string finalRes = "{\"sequenceNumber\":" + std::to_string(curSequenceNumber.fetch_add(1)) + R"(,"response":{"requestId":)" + std::to_string(requestId) + R"(,"type":")" + resType + R"(","payload":)";
    finalRes += resPayload + "}}";
    return finalRes;
}

std::string SpectreWebsocket::FormulateFinalNotification(Notification& notification) {
    return "{\"sequenceNumber\":" + std::to_string(curSequenceNumber.fetch_add(1)) + ",\"notification\":{\"type\":\"" + notification.GetNotificationType().GetName() + "\",\"payload\":" + notification.GetNotificationData() + "}}";
}

const std::string& SpectreWebsocket::GetPlayerId() {
    return playerId;
}

std::optional<SpectreWebsocket*> SpectreWebsocket::GetConnectionForPlayer(const std::string& playerId) {
    std::unique_lock lock(connectionsMapMutex);
    auto it = connectionsByPlayerId.find(playerId);
    if (it == connectionsByPlayerId.end()) {
        return std::nullopt;
    }
    return it->second;
}

void SpectreWebsocket::ScheduleNotification(Notification notif) {
    std::unique_lock lock(notificationQueueLock);
    notificationsToDeliver.push(notif);
}

void SpectreWebsocket::ScheduleNotificationForPlayer(const std::string& playerId, Notification notif) {
    std::optional<SpectreWebsocket*> connection = GetConnectionForPlayer(playerId);
    if (!connection.has_value()) {
        // player is not logged in, save the notification to disk instead
        std::unique_ptr<SavedNotificationData> notifData = PlayerDatabase::Get().GetField<SavedNotificationData>(FieldKey::NOTIFICATION_DATA, playerId);
        SavedNotification* newNotif = notifData->add_notificationstodeliver();
        newNotif->set_notificationdata(notif.GetNotificationData());
        newNotif->set_notificationid(notif.GetNotificationId());
        newNotif->set_rpctype(notif.GetNotificationType().GetName());
        PlayerDatabase::Get().SetField(FieldKey::NOTIFICATION_DATA, notifData.get(), playerId);
    } else {
        connection.value()->ScheduleNotification(std::move(notif));
    }
}