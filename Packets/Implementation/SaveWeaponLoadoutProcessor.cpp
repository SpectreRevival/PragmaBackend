#include <CaseHelper.h>
#include <PlayerDatabase.h>
#include <SaveWeaponLoadoutMessage.pb.h>
#include <SaveWeaponLoadoutProcessor.h>
#include <WeaponLoadout.pb.h>

SaveWeaponLoadoutProcessor::SaveWeaponLoadoutProcessor(SpectreRpcType rpcType)
    : WebsocketPacketProcessor(rpcType) {
}

std::optional<WebsocketPayload> SaveWeaponLoadoutProcessor::Process(SpectreWebsocketRequest& packet) {
    std::unique_ptr<SaveWeaponLoadoutMessage> dataToSave = packet.GetPayloadAsMessage<SaveWeaponLoadoutMessage>();
    const WeaponLoadout& loadoutToSave = dataToSave->weaponloadoutdata();

    std::unique_ptr<WeaponLoadouts> loadouts = PlayerDatabase::Get().GetField<WeaponLoadouts>(
        FieldKey::PLAYER_WEAPON_LOADOUT,
        packet.GetPlayerId());

    if (!loadouts) loadouts = std::make_unique<WeaponLoadouts>();

    bool dataWritten = false;
    for (int i = 0; i < loadouts->weaponloadoutdata_size(); i++) {

        if (CaseInsensitiveEquals(loadouts->weaponloadoutdata(i).loadoutid(), loadoutToSave.loadoutid())) {

            loadouts->mutable_weaponloadoutdata(i)->CopyFrom(loadoutToSave);
            loadouts->mutable_weaponloadoutdata(i)->set_playerid(packet.GetPlayerId());

            dataWritten = true;
            break;
        }
    }
    if (!dataWritten) {
        spdlog::warn("didn't find the weapon loadout the game was trying to edit, added it as a new loadout\nLoadout id: {}", loadoutToSave.loadoutid());
        WeaponLoadout* newLoadout = loadouts->add_weaponloadoutdata();
        newLoadout->CopyFrom(loadoutToSave);
        newLoadout->set_playerid(packet.GetPlayerId());
    }
    PlayerDatabase::Get().SetField(FieldKey::PLAYER_WEAPON_LOADOUT, loadouts.get(), packet.GetPlayerId());
    nlohmann::json res{};
    res["success"] = true;
    res["savedLoadoutId"] = loadoutToSave.loadoutid();
    return res;
}