#pragma once
#include <SpectreRpcType.h>
#include <google/protobuf/message.h>
#include <nlohmann/json.hpp>
#include <restinio/string_view.hpp> // DNR until https://github.com/Stiffstream/restinio/pull/243 is merged
#include <restinio/websocket/websocket.hpp>

using json = nlohmann::ordered_json;
namespace pbuf = google::protobuf;
namespace rws = restinio::websocket::basic;

class SpectreWebsocket {
    friend class RequestRouter;
  private:
    int curSequenceNumber;
    std::string playerId;
    // WARNING: NOT SET IN THE CONSTRUCTOR, BUT IMMEDIATELY AFTER
    rws::ws_handle_t websocketHandle;

  public:
    SpectreWebsocket(restinio::request_handle_t initiationRequest);
    /*
        Warning: Do not send packets through the socket directly, it bypasses abstraction and will cause bad things to happen
    */
    void OnReceiveWebsocketMessage(rws::ws_handle_t websocketHandler, rws::message_handle_t message);

    std::string FormulateFinalResponse(const std::shared_ptr<json>& res);

    std::string FormulateFinalResponse(const std::string& resPayload, int requestId, const std::string& resType);

    std::string FormulateFinalResponse(const pbuf::Message& payload, const std::string& resType, int requestId);

    const std::string& GetPlayerId();
};