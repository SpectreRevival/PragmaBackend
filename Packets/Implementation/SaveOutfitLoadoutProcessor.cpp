#include <CaseHelper.h>
#include <OutfitLoadout.pb.h>
#include <PlayerDatabase.h>
#include <SaveOutfitLoadoutProcessor.h>

SaveOutfitLoadoutProcessor::SaveOutfitLoadoutProcessor(SpectreRpcType rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> SaveOutfitLoadoutProcessor::Process(SpectreWebsocketRequest& packet) {
    std::unique_ptr<OutfitLoadout> loadoutToSave = packet.GetPayloadAsMessage<OutfitLoadout>();
    std::unique_ptr<OutfitLoadouts> loadouts = PlayerDatabase::Get().GetField<OutfitLoadouts>(FieldKey::PLAYER_OUTFIT_LOADOUT, packet.GetPlayerId());
    if (!loadouts) loadouts = std::make_unique<OutfitLoadouts>();

    bool dataWritten = false;
    for (int i = 0; i < loadouts->loadouts_size(); i++) {
        if (CaseInsensitiveEquals(loadouts->loadouts(i).loadoutid(), loadoutToSave->loadoutid())) {

            loadouts->mutable_loadouts(i)->CopyFrom(*loadoutToSave);
            loadouts->mutable_loadouts(i)->set_playerid(packet.GetPlayerId());

            dataWritten = true;
            break;
        }
    }
    if (!dataWritten) {
        spdlog::warn("didn't find the outfit loadout the game was trying to edit, added it as a new loadout\nLoadout id: {}", loadoutToSave->loadoutid());
        OutfitLoadout* newLoadout = loadouts->add_loadouts();
        newLoadout->CopyFrom(*loadoutToSave);
        newLoadout->set_playerid(packet.GetPlayerId());
    }
    PlayerDatabase::Get().SetField(FieldKey::PLAYER_OUTFIT_LOADOUT, loadouts.get(), packet.GetPlayerId());
    nlohmann::json res;
    res["success"] = true;
    res["savedLoadoutId"] = loadoutToSave->loadoutid();
    return res;
}