#pragma once
#include <string>
#include <Notification.h>
#include <queue>
#include <memory>

class PlayerConnectionThread {
private:
    boost::asio::io_context& ioCtx;
    std::queue<std::unique_ptr<Notification>> notificationQueue;
    std::mutex notificationQueueMutex;
    boost::beast::flat_buffer httpDataBuffer;
    http::request<http::string_body> httpRequest;
    boost::beast::flat_buffer websocketDataBuffer;
    void BeginHTTPRead();
    void BeginWebsocketRead();
    tcp::socket playerConnectionSocket;
    SpectreWebsocket* websocketConnection{};
    std::mutex websocketConnectionMutex;
    boost::beast::websocket::stream<tcp::socket>* ws{};
    void OnWebsocketMessageReceive(boost::beast::error_code err, std::size_t readSize);
    void OnHTTPRequestReceive(boost::beast::error_code err, std::size_t readSize);
    static std::vector<PlayerConnectionThread*> playerConnections;
    std::mutex playerConnectionsMutex;
    void NotificationSenderThread(std::stop_token st);
    std::jthread notificationSenderThread;
public:
    explicit PlayerConnectionThread(tcp::socket socket, boost::asio::io_context& ioCtx);
    ~PlayerConnectionThread();
    PlayerConnectionThread(PlayerConnectionThread& other) = delete;
    PlayerConnectionThread() = delete;
    void EnqueueNotification(std::unique_ptr<Notification> notification);
    const std::string& GetPlayerId() const;
    SpectreWebsocket* GetWebsocketConnection();
    std::string GetIPAddress() const;
    unsigned int GetPort() const;
    bool IsWebsocketConnection() const;
    static std::vector<PlayerConnectionThread*>& GetPlayerConnections();
};