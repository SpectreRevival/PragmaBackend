#include <GameConnectionDetails.h>
#include <GetHostConnectionDetailsProcessor.h>

GetHostConnectionDetailsProcessor::GetHostConnectionDetailsProcessor(const SpectreRpcType& rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> GetHostConnectionDetailsProcessor::Process(SpectreWebsocketRequest& packet) {
    (void)packet;
    return GameConnectionDetails::FromEnvironment().ToHostConnectionDetailsPayload();
}
