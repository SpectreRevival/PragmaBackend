#include "PacketProcessor.h"
#include "PlayerDatabase.h"
#include "SavedNotificationData.pb.h"

#include <PlayerConnectionThread.h>

std::vector<PlayerConnectionThread*> PlayerConnectionThread::playerConnections{};

PlayerConnectionThread::PlayerConnectionThread(tcp::socket socket, boost::asio::io_context& ioCtx)
    : playerConnectionSocket(std::move(socket)), ioCtx(ioCtx) {
    std::unique_lock lock(playerConnectionsMutex);
    playerConnections.push_back(this);
    BeginHTTPRead();
}

const std::string& PlayerConnectionThread::GetPlayerId() const {
    static const std::string emptyId = "";
    if (websocketConnection == nullptr) {
        spdlog::error("Cannot return valid player id in PlayerConnectionThread::GetPlayerId() since websocket conn hasn't been initialized yet, returning empty id");
        return emptyId;
    }
    return websocketConnection->GetPlayerId();
}

SpectreWebsocket* PlayerConnectionThread::GetWebsocketConnection() {
    return websocketConnection;
}

std::string PlayerConnectionThread::GetIPAddress() const {
    if (!IsWebsocketConnection()) {
        return playerConnectionSocket.remote_endpoint().address().to_string();
    }
    return ws->next_layer().remote_endpoint().address().to_string();
}

unsigned int PlayerConnectionThread::GetPort() const {
    if (!IsWebsocketConnection()) {
        return playerConnectionSocket.remote_endpoint().port();
    }
    return ws->next_layer().remote_endpoint().port();
}

void PlayerConnectionThread::EnqueueNotification(std::unique_ptr<Notification> notification) {
    std::unique_lock lock(notificationQueueMutex);
    notificationQueue.push(std::move(notification));
}

void PlayerConnectionThread::BeginHTTPRead() {
    http::async_read(playerConnectionSocket, httpDataBuffer, httpRequest,
    boost::beast::bind_front_handler(&PlayerConnectionThread::OnHTTPRequestReceive, this));
}

void PlayerConnectionThread::BeginWebsocketRead() {
    ws->async_read(websocketDataBuffer, boost::beast::bind_front_handler(&PlayerConnectionThread::OnWebsocketMessageReceive, this));
}

static std::string StripQueryParams(const std::string& url) {
    size_t pos = url.find('?');
    if (pos != std::string::npos) {
        if (url.at(0) == '/' && url.at(1) == '/') {
            return url.substr(1, pos);
        }
        return url.substr(0, pos);
    }
    if (url.at(0) == '/' && url.at(1) == '/') {
        return url.substr(1);
    }
    return url;
}

void PlayerConnectionThread::OnHTTPRequestReceive(boost::beast::error_code err, std::size_t readSize) {
    if (err.failed()) {
        spdlog::error("Failed to receive HTTP request due to error: {}", err.message());
        return;
    }
    if (boost::beast::websocket::is_upgrade(httpRequest)) {
        spdlog::info("Upgrading connection with {}:{} to websocket", GetIPAddress(), GetPort());
        ws = new boost::beast::websocket::stream<tcp::socket>(std::move(playerConnectionSocket));
        ws->accept(httpRequest);
        websocketConnection = new SpectreWebsocket(*ws, httpRequest);
        std::unique_ptr<SavedNotificationData> savedNotifs = PlayerDatabase::Get().GetField<SavedNotificationData>(FieldKey::NOTIFICATION_DATA, GetPlayerId());
        for (int i = 0; i < savedNotifs->notificationstodeliver_size(); i++) {
            const SavedNotification& savedNotif = savedNotifs->notificationstodeliver(i);
            std::unique_ptr<Notification> notification = std::make_unique<Notification>(SpectreRpcType(savedNotif.rpctype()), savedNotif.notificationid(), savedNotif.notificationdata());
            notificationQueue.push(std::move(notification));
        }
        notificationSenderThread = std::jthread([this](std::stop_token st) {
            NotificationSenderThread(st);
        });
        BeginWebsocketRead();
        return;
    }
    std::string target = StripQueryParams(httpRequest.target());
    HTTPPacketProcessor* processor = HTTPPacketProcessor::GetProcessorForRoute(target);
    if (processor == nullptr) {
        spdlog::warn("Missing a handler for HTTP route {}", target);
        http::response<http::string_body> res;
        res.version(httpRequest.version());
        res.keep_alive(httpRequest.keep_alive());
        res.result(http::status::not_found);
        res.set(http::field::content_type, "application/json; charset=UTF-8");
        res.body() = "{}";
        res.prepare_payload();
        http::write(playerConnectionSocket, res);
        return;
    }
    processor->Process(httpRequest, playerConnectionSocket);
    BeginHTTPRead();
}

void PlayerConnectionThread::OnWebsocketMessageReceive(boost::beast::error_code err, std::size_t readSize) {
    if (err.failed()) {
        spdlog::error("Failed to receive websocket message: {}", err.message());
        return;
    }
    std::unique_lock wsLock(websocketConnectionMutex);
    std::string msgData = boost::beast::buffers_to_string(websocketDataBuffer.data());
    SpectreWebsocketRequest wsReq(*websocketConnection, msgData);
    websocketDataBuffer.consume(websocketDataBuffer.size());
    WebsocketPacketProcessor* processor = WebsocketPacketProcessor::GetProcessorForRpc(wsReq.GetRequestType());
    if (processor == nullptr) {
        spdlog::warn("No processor for message type {}, dropping packet", wsReq.GetRequestType().GetName());
        return;
    }
    processor->Process(wsReq, *websocketConnection);
    BeginWebsocketRead();
}

void PlayerConnectionThread::NotificationSenderThread(std::stop_token st) {
    while (!st.stop_requested()) {
        if (!notificationQueueMutex.try_lock()) {
            continue;
        }
        if (notificationQueue.empty()) {
            notificationQueueMutex.unlock();
            continue;
        }
        std::unique_ptr<Notification> notif = std::move(notificationQueue.front());
        notificationQueue.pop();
        std::unique_lock conLock(websocketConnectionMutex);
        websocketConnection->SendNotification(notif->GetNotificationData(), notif->GetNotificationType());
        notificationQueueMutex.unlock();
    }
}

PlayerConnectionThread::~PlayerConnectionThread() {
    notificationSenderThread.request_stop();
    notificationSenderThread.join();
    if (!GetPlayerId().empty()) {
        std::unique_ptr<SavedNotificationData> notificationData = PlayerDatabase::Get().GetField<SavedNotificationData>(FieldKey::NOTIFICATION_DATA, GetPlayerId());
        std::unique_lock Notiflock(notificationQueueMutex);
        while (!notificationQueue.empty()){
            Notification& notif = *notificationQueue.front();
            SavedNotification notifSaved;
            notifSaved.set_notificationid(notif.GetNotificationId());
            notifSaved.set_rpctype(notif.GetNotificationType().GetName());
            notifSaved.set_notificationdata(notif.GetNotificationData());
            notificationData->add_notificationstodeliver()->CopyFrom(notifSaved);
            notificationQueue.pop();
        }
        PlayerDatabase::Get().SetField(FieldKey::NOTIFICATION_DATA, notificationData.get(), GetPlayerId());
    }
    delete websocketConnection;
    delete ws;
}

bool PlayerConnectionThread::IsWebsocketConnection() const {
    return ws != nullptr;
}

std::vector<PlayerConnectionThread*>& PlayerConnectionThread::GetPlayerConnections() {
    return playerConnections;
}