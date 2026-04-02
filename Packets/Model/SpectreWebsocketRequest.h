#pragma once
#include <SpectreRpcType.h>
#include <boost/beast/core.hpp>
#include <google/protobuf/util/json_util.h>
#include <nlohmann/json.hpp>
#include <spdlog/spdlog.h>

namespace pbuf = google::protobuf;
using reqbuf = boost::beast::flat_buffer;

class SpectreWebsocketRequest {
  private:
    std::string reqBody;
    SpectreRpcType requestType;
    std::shared_ptr<nlohmann::json> reqJson;
    std::string payloadAsStr;
    int requestId;

  public:
    SpectreWebsocketRequest(std::string& reqBody);

    std::shared_ptr<nlohmann::json> GetPayload() const;

    template <typename T>
    std::unique_ptr<T> GetPayloadAsMessage() {
        static_assert(std::is_base_of_v<pbuf::Message, T>, "Type passed to GetPayloadAsMessage must be subclass of pbuf::Message");
        T message;
        auto status = pbuf::util::JsonStringToMessage(
            payloadAsStr,
            &message);
        if (!status.ok()) {
            spdlog::error("Failed to parse incoming request to message: {}", status.message());
            throw;
        }
        return std::make_unique<T>(message);
    }

    [[nodiscard]] SpectreRpcType GetRequestType() const;

    std::string GetResponseType() const;

    [[nodiscard]] int GetRequestId() const;

    const std::string& GetBody() const;
};