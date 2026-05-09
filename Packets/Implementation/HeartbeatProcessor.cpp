#include <HeartbeatProcessor.h>

HeartbeatProcessor::HeartbeatProcessor(const SpectreRpcType& rpcType)
    : WebsocketPacketProcessor(rpcType) {};

std::optional<WebsocketPayload> HeartbeatProcessor::Process(SpectreWebsocketRequest& /*packet*/) {
    return nlohmann::json::object();
}