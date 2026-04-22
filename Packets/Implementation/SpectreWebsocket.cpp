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
#include <utility>

std::unordered_map<std::string, WebSocketConnectionPtr> SpectreWebsocketController::connectionsByPlayerId{};
std::mutex SpectreWebsocketController::connectionsMapMutex{};

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

void SpectreWebsocket::NotificationThread(const std::stop_token& st) {
    while (!st.stop_requested()) {
        std::unique_lock lock(notificationQueueLock);
        notificationQueueCondition.wait(lock, [&] {
            return st.stop_requested() || (!notificationsToDeliver.empty() && !con.expired() && con.lock()->connected());
        });
        std::shared_ptr<WebSocketConnection> connection = con.lock();
        if (st.stop_requested()) break;

        Notification notification = notificationsToDeliver.front();
        notificationsToDeliver.pop_front();

        connection->send(FormulateFinalNotification(notification));

        SavedNotificationData savedData;
        for (const Notification& notif : notificationsToDeliver) {
            auto* newNotif = savedData.add_notificationstodeliver();
            newNotif->set_notificationdata(notif.GetNotificationData());
            newNotif->set_notificationid(notif.GetNotificationId());
            newNotif->set_rpctype(notif.GetNotificationType().GetName());
        }

        PlayerDatabase::Get().SetField(FieldKey::NOTIFICATION_DATA, &savedData, playerId);
    }
}

void SpectreWebsocketController::handleNewMessage(const drogon::WebSocketConnectionPtr& wsCon, std::string&& message, const drogon::WebSocketMessageType& messageType) {
    if (messageType != WebSocketMessageType::Binary && messageType != WebSocketMessageType::Text) {
        return;
    }
    std::shared_ptr<SpectreWebsocket> ctx = wsCon->getContext<SpectreWebsocket>();
    std::string playerId = ctx->GetPlayerId();
    SpectreWebsocketRequest request(message, playerId);
    WebsocketPacketProcessor* processor = WebsocketPacketProcessor::GetProcessorForRpc(request.GetRequestType());
    if (processor == nullptr) {
        spdlog::warn("Failed to find websocket message for rpc type {}", request.GetRequestType().GetName());
        return;
    }
    std::optional<WebsocketPayload> response = processor->Process(request);
    if (!response.has_value()) {
        return;
    }
    std::string finalResponse = ctx->FormulateFinalResponse(response.value().GetPayload(), request.GetRequestId(), request.GetResponseType());
    wsCon->send(finalResponse);
    for (const Notification& notification : response.value().GetPostSendNotifications()) {
        ScheduleNotificationForPlayer(ctx->GetPlayerId(), notification);
    }
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
    return "{\"sequenceNumber\":" + std::to_string(curSequenceNumber.fetch_add(1)) + R"(,"notification":{"type":")" + notification.GetNotificationType().GetName() + R"(","payload":)" + notification.GetNotificationData() + "}}";
}

const std::string& SpectreWebsocket::GetPlayerId() {
    return playerId;
}

std::optional<WebSocketConnectionPtr> SpectreWebsocketController::GetConnectionForPlayer(const std::string& playerId) {
    std::unique_lock lock(connectionsMapMutex);
    auto it = connectionsByPlayerId.find(playerId);
    if (it == connectionsByPlayerId.end()) {
        return std::nullopt;
    }
    if (!it->second) {
        return std::nullopt;
    }
    return it->second;
}

void SpectreWebsocket::ScheduleNotification(const Notification& notif) {
    {
        std::lock_guard lock(notificationQueueLock);
        notificationsToDeliver.push_back(notif);
    }
    notificationQueueCondition.notify_one();
}

void SpectreWebsocketController::ScheduleNotificationForPlayer(const std::string& playerId, const Notification& notif) {
    std::optional<WebSocketConnectionPtr> connection = GetConnectionForPlayer(playerId);
    if (!connection.has_value()) {
        // player is not logged in, save the notification to disk instead
        std::unique_ptr<SavedNotificationData> notifData = PlayerDatabase::Get().GetField<SavedNotificationData>(FieldKey::NOTIFICATION_DATA, playerId);
        SavedNotification* newNotif = notifData->add_notificationstodeliver();
        newNotif->set_notificationdata(notif.GetNotificationData());
        newNotif->set_notificationid(notif.GetNotificationId());
        newNotif->set_rpctype(notif.GetNotificationType().GetName());
        PlayerDatabase::Get().SetField(FieldKey::NOTIFICATION_DATA, notifData.get(), playerId);
    } else {
        connection.value()->getContext<SpectreWebsocket>()->ScheduleNotification(notif);
    }
}

SpectreWebsocket::SpectreWebsocket(const drogon::HttpRequestPtr& req, const drogon::WebSocketConnectionPtr& connection)
    : con(connection), curSequenceNumber(0) {

    const auto bearer = ExtractBearer(req->getHeader("Authorization"));
    const auto pid = bearer.empty() ? std::string() : DecodePlayerIdNoverify(bearer);

    if (!pid.empty()) {
        playerId = pid;
    } else {
        spdlog::error("no playerid ???? investigate me!, thinks will be SEVERELY wrong for connection with {}", req->peerAddr().toIpPort());
        playerId = "1";
    }
    std::unique_ptr<SavedNotificationData> notificationsFromDisk = PlayerDatabase::Get().GetField<SavedNotificationData>(FieldKey::NOTIFICATION_DATA, playerId);
    for (int i = 0; i < notificationsFromDisk->notificationstodeliver_size(); i++) {
        const SavedNotification& currentNotif = notificationsFromDisk->notificationstodeliver(i);
        notificationsToDeliver.emplace_back(SpectreRpcType(currentNotif.rpctype()), currentNotif.notificationid(), currentNotif.notificationdata());
    }
    notificationWorkerThread = std::jthread([this](const std::stop_token& st) {
        NotificationThread(st);
    });
    SpectreWebsocketController::AddConnection(playerId, connection);
}

void SpectreWebsocketController::AddConnection(const std::string& playerId, WebSocketConnectionPtr con) {
    std::unique_lock connectionsMapLock(connectionsMapMutex);
    connectionsByPlayerId.insert_or_assign(playerId, con);
}

void SpectreWebsocketController::handleNewConnection(const drogon::HttpRequestPtr& req, const drogon::WebSocketConnectionPtr& con) {
    std::shared_ptr<SpectreWebsocket> wsCtx = std::make_shared<SpectreWebsocket>(req, con);
    con->setContext(wsCtx);
}

void SpectreWebsocketController::handleConnectionClosed(const WebSocketConnectionPtr& conPtr) {
}