#pragma once
#include <SpectreRpcType.h>
#include <google/protobuf/message.h>
#include <nlohmann/json.hpp>
#include <restinio/websocket/websocket.hpp>
#include <Notification.h>
#include <functional>

using json = nlohmann::ordered_json;
namespace pbuf = google::protobuf;
namespace rws = restinio::websocket::basic;

class SpectreWebsocket {
    friend class RequestRouter;
private:
    static std::unordered_map<std::string, SpectreWebsocket*> connectionsByPlayerId;
    static std::mutex connectionsMapMutex;

    std::atomic_int curSequenceNumber;
    std::string playerId;
    // WARNING: NOT SET IN THE CONSTRUCTOR, BUT IMMEDIATELY AFTER
    rws::ws_handle_t websocketHandle;
    std::queue<Notification> notificationsToDeliver;
    std::mutex notificationQueueLock;
    std::jthread notificationWorkerThread;
    void NotificationThread(std::stop_token st);

  public:
    explicit SpectreWebsocket(restinio::request_handle_t initiationRequest);

    void OnReceiveWebsocketMessage(rws::ws_handle_t websocketHandler, rws::message_handle_t message);

    std::string FormulateFinalResponse(const std::shared_ptr<json>& res);

    std::string FormulateFinalResponse(const std::string& resPayload, int requestId, const std::string& resType);

    std::string FormulateFinalResponse(const pbuf::Message& payload, const std::string& resType, int requestId);

    std::string FormulateFinalNotification(Notification& notification);

    const std::string& GetPlayerId();

    void ScheduleNotification(Notification notif);

    static std::optional<SpectreWebsocket*> GetConnectionForPlayer(const std::string& playerId);
    static void ScheduleNotificationForPlayer(const std::string& playerId, Notification notif);
};