#pragma once
#include <Notification.h>
#include <SpectreRpcType.h>
#include <drogon/WebSocketController.h>
#include <functional>
#include <drogon/drogon.h>
#include <google/protobuf/message.h>
#include <nlohmann/json.hpp>

using json = nlohmann::ordered_json;
namespace pbuf = google::protobuf;
using namespace drogon;

class SpectreWebsocket {
    std::atomic_int curSequenceNumber;
    std::string playerId;
    std::deque<Notification> notificationsToDeliver;
    std::mutex notificationQueueLock;
    std::jthread notificationWorkerThread;
    WebSocketConnectionPtr con;
    void NotificationThread(const std::stop_token& st);
public:
    explicit SpectreWebsocket(const drogon::HttpRequestPtr& req, const drogon::WebSocketConnectionPtr& con);


    std::string FormulateFinalResponse(const std::shared_ptr<json>& res);

    std::string FormulateFinalResponse(const std::string& resPayload, int requestId, const std::string& resType);

    std::string FormulateFinalResponse(const pbuf::Message& payload, const std::string& resType, int requestId);

    std::string FormulateFinalNotification(Notification& notification);

    const std::string& GetPlayerId();

    void ScheduleNotification(const Notification& notif);
};

class SpectreWebsocketController : public drogon::WebSocketController<SpectreWebsocketController> {
    WS_PATH_LIST_BEGIN
    WS_PATH_ADD("/");
    WS_PATH_LIST_END
private:
    static std::unordered_map<std::string, WebSocketConnectionPtr> connectionsByPlayerId;
    static std::mutex connectionsMapMutex;
public:
    void handleNewConnection(const drogon::HttpRequestPtr& req, const drogon::WebSocketConnectionPtr& con) override;
    void handleNewMessage(const drogon::WebSocketConnectionPtr& con, std::string &&message, const drogon::WebSocketMessageType &) override;
    static std::optional<SpectreWebsocket*> GetConnectionForPlayer(const std::string& playerId);
    static void ScheduleNotificationForPlayer(const std::string& playerId, const Notification& notif);
    static void AddConnection(const std::string& playerId, WebSocketConnectionPtr con);
};