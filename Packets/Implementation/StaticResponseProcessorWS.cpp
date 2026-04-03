#include <StaticResponseProcessorWS.h>
#include <nlohmann/json.hpp>

StaticResponseProcessorWS::StaticResponseProcessorWS(const SpectreRpcType rpcType, const nlohmann::json& res)
    : WebsocketPacketProcessor(rpcType), staticRes(res.dump()){

}

std::optional<WebsocketPayload> StaticResponseProcessorWS::Process(SpectreWebsocketRequest& packet) {
    return staticRes;
}