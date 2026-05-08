#include "PlayerDatabase.h"

#include "PersistenceUtilities.h"

#include <FriendsList.pb.h>
#include <Inventory.pb.h>
#include <LegacyPlayerData.pb.h>
#include <OutfitLoadout.pb.h>
#include <PlayerData.pb.h>
#include <PlayerPresence.pb.h>
#include <ProfileData.pb.h>
#include <ResourcesUtilities.h>
#include <SavedNotificationData.pb.h>
#include <WeaponLoadout.pb.h>

PlayerDatabase::PlayerDatabase(const fs::path& path)
    : ProtobufDatabase(path, "players", "PlayerID", "TEXT") {
    AddPrototype(FieldKey::PLAYER_INVENTORY, ProtobufDatabaseFieldData::WithDefaultFromPath<Inventory>(FieldKey::PLAYER_INVENTORY, "inventory", ResourcesUtilities::GetResourcesFolder() / "payloads" / "ws" / "game" / "DefaultInventory.json"));
    AddPrototype(FieldKey::PLAYER_OUTFIT_LOADOUT, ProtobufDatabaseFieldData::WithDefaultFromPath<OutfitLoadouts>(FieldKey::PLAYER_OUTFIT_LOADOUT, "outfitLoadouts", ResourcesUtilities::GetResourcesFolder() / "payloads" / "ws" / "game" / "DefaultOutfitLoadout.json"));
    AddPrototype(FieldKey::PLAYER_WEAPON_LOADOUT, ProtobufDatabaseFieldData::WithDefaultFromPath<WeaponLoadouts>(FieldKey::PLAYER_WEAPON_LOADOUT, "weaponLoadouts", ResourcesUtilities::GetResourcesFolder() / "payloads" / "ws" / "game" / "DefaultWeaponLoadout.json"));
    AddPrototype(FieldKey::PLAYER_DATA, ProtobufDatabaseFieldData::WithDefaultFromPath<PlayerData>(FieldKey::PLAYER_DATA, "playerData", ResourcesUtilities::GetResourcesFolder() / "payloads" / "ws" / "game" / "DefaultPlayerData.json"));
    AddPrototype(FieldKey::PROFILE_DATA, ProtobufDatabaseFieldData::WithDefaultFromPath<ProfileData>(FieldKey::PROFILE_DATA, "profileData", ResourcesUtilities::GetResourcesFolder() / "payloads" / "ws" / "game" / "DefaultProfile.json"));
    AddPrototype(FieldKey::PLAYER_LEGACY_DATA, ProtobufDatabaseFieldData::WithDefaultFromPath<LegacyPlayerData>(FieldKey::PLAYER_LEGACY_DATA, "legacyData", ResourcesUtilities::GetResourcesFolder() / "payloads" / "ws" / "game" / "DefaultLegacyData.json"));
    AddPrototype(FieldKey::PLAYER_PRESENCE, ProtobufDatabaseFieldData::WithDefaultFromPath<PlayerPresence>(FieldKey::PLAYER_PRESENCE, "presence", ResourcesUtilities::GetResourcesFolder() / "payloads" / "ws" / "game" / "DefaultPlayerPresence.json"));
    AddPrototype(FieldKey::FRIENDS_LIST, ProtobufDatabaseFieldData::WithDefaultFromPath<FriendsList>(FieldKey::FRIENDS_LIST, "friendsList", ResourcesUtilities::GetResourcesFolder() / "payloads" / "ws" / "game" / "DefaultFriendsList.json"));
    AddPrototype(FieldKey::NOTIFICATION_DATA, ProtobufDatabaseFieldData::WithDefaultFromPath<SavedNotificationData>(FieldKey::NOTIFICATION_DATA, "notificationData", ResourcesUtilities::GetResourcesFolder() / "payloads" / "ws" / "game" / "DefaultSavedNotifications.json"));
}

PlayerDatabase& PlayerDatabase::Get() {
    static PlayerDatabase inst = []() {
        PlayerDatabase db{PersistenceUtilities::GetSavePath() / "playerdata.sqlite"};
        return db;
    }();
    return inst;
}