#include <EnterMatchmakingProcessor.h>
#include <EnterMatchmakingRequest.pb.h>
#include <GameConnectionDetails.h>
#include <Notification.h>
#include <PartyDatabase.h>

EnterMatchmakingProcessor::EnterMatchmakingProcessor(const SpectreRpcType& rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> EnterMatchmakingProcessor::Process(SpectreWebsocketRequest& packet) {
    const std::unique_ptr<EnterMatchmakingRequest> req = packet.GetPayloadAsMessage<EnterMatchmakingRequest>();
    const PartyResponse res = PartyDatabase::Get().GetPartyRes(req->partyid());

    const GameConnectionDetails details = BuildGameConnectionDetailsFromEnv();
    return WebsocketPayload(
        PartyDatabase::SerializePartyToString(res),
        {
            Notification(
                SpectreRpcType("GameInstanceRpc.AddedToGameV1Notification"),
                details.gameinstanceid() + "-added",
                SerializeAddedToGameNotification(details)),
            Notification(
                SpectreRpcType("GameInstanceRpc.HostConnectionDetailsV1Notification"),
                details.gameinstanceid() + "-host",
                SerializeHostConnectionDetailsNotification(details)),
        });
}
