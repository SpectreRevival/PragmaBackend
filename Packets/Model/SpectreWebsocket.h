#pragma once
#include <SpectreRpcType.h>
#include <boost/asio.hpp>
#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/beast/websocket/ssl.hpp>
#include <google/protobuf/message.h>
#include <nlohmann/json.hpp>
#include <restinio/all.hpp>

using tcp = boost::asio::ip::tcp;
namespace http = boost::beast::http;
using json = nlohmann::ordered_json;
namespace pbuf = google::protobuf;
namespace rws = restinio::websocket::basic;

class SpectreWebsocket {
    friend class RequestRouter;
  private:
    int curSequenceNumber;
    std::string playerId;
    rws::ws_handle_t websocketHandle;

  public:
    SpectreWebsocket(restinio::request_handle_t initiationRequest);
    /*
        Warning: Do not send packets through the socket directly, it bypasses abstraction and will cause bad things to happen
    */
    void OnReceiveWebsocketMessage(rws::ws_handle_t websocketHandler, rws::message_handle_t message);

    void SendPacket(const std::shared_ptr<json>& res);

    void SendPacket(const std::string& resPayload, int requestId, const std::string& resType);

    void SendPacket(const pbuf::Message& payload, const std::string& resType, int requestId);

    void SendNotification(const std::shared_ptr<json>& notif, const SpectreRpcType& notificationType);

    void SendNotification(const std::string& notifPayload, const SpectreRpcType& notificationType);

    void SendNotification(const pbuf::Message& notif, const SpectreRpcType& notificationType);

    const std::string& GetPlayerId();
};