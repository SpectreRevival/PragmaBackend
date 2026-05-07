#include "BanDatabase.h"
#include "ProviderLinkDatabase.h"
#include "ResourcesUtilities.h"

#include <AuthenticateHandler.h>
#include <OutfitLoadout.pb.h>
#include <PlayerData.pb.h>
#include <PlayerDatabase.h>
#include <ProfileData.pb.h>
#include <SteamAuthTicket.h>
#include <WeaponLoadout.pb.h>
#include <boost/lexical_cast.hpp>
#include <boost/uuid/name_generator_sha1.hpp>
#include <boost/uuid/random_generator.hpp>
#include <boost/uuid/string_generator.hpp>
#include <boost/uuid/uuid.hpp>
#include <boost/uuid/uuid_io.hpp>
#include <fstream>
#include <jwt-cpp/jwt.h>
#include <nlohmann/json.hpp>
#include <random>
#include <string>
#include <utility>

AuthenticateHandler::AuthenticateHandler(HTTPRequestIdentifier id)
    : HTTPPacketProcessor(std::move(id)) {
}

static std::string PlayerUuidFromSteam64(const std::string& steam64) {
    static const auto ns = boost::uuids::string_generator{}("c8a6b6ce-1e7b-49f2-9a4f-0be3d7b7e5a1");
    const auto id = boost::uuids::name_generator_sha1{ns}(steam64);
    return boost::lexical_cast<std::string>(id);
}

std::optional<drogon::HttpResponsePtr> AuthenticateHandler::Process(const drogon::HttpRequestPtr& req) {
    const auto body = nlohmann::json::parse(req->body(), nullptr, false);
    if (body.is_discarded() || !body.contains("providerToken") || !body.at("providerToken").is_string()) {
        auto res = HttpResponse::newHttpResponse();
        res->setBody(R"({"error":"NOSTEAMID"})");
        res->setStatusCode(k400BadRequest);
        return res;
    }

    const std::string steam64 = SteamAuthTicket::ExtractSteamId64(body.at("providerToken").get<std::string>());
    if (steam64.empty()) {
        auto res = HttpResponse::newHttpResponse();
        res->setBody(R"({"error":"NOSTEAMID"})");
        res->setStatusCode(k400BadRequest);
        return res;
    }

    auto playerId = ProviderLinkDatabase::Get().LookupPlayerByProvider(AuthProvider::STEAM, steam64);
    if (playerId.empty()) {
        playerId = CreatePlayerFromSteam(steam64, "Player");
        ProviderLinkDatabase::Get().UpsertProviderMap(AuthProvider::STEAM, steam64, playerId);
    }

    if (BanDatabase::Get().IsBanned(playerId)) {
        auto res = HttpResponse::newHttpResponse();
        res->setBody(R"({"error":"ACCOUNT BANNED. CONTACT ASTROVAL0 ON DISCORD"})");
        res->setStatusCode(k403Forbidden);
        return res;
    }

    auto prof = PlayerDatabase::Get().GetField<ProfileData>(FieldKey::PROFILE_DATA, playerId);
    const std::string display = prof ? prof->displayname().displayname() : "Player";
    const std::string disc = prof ? prof->displayname().discriminator() : "0000";
    const std::string socialId = playerId;

    nlohmann::json tokens = {
        {"pragmaGameToken", BuildJwt("GAME", playerId, socialId, display, disc)},
        {"pragmaSocialToken", BuildJwt("SOCIAL", playerId, socialId, display, disc)}};

    nlohmann::json out = {{"pragmaTokens", tokens}};
    auto res = HttpResponse::newHttpResponse();
    res->setBody(out.dump());
    return res;
}

std::string AuthenticateHandler::CreatePlayerFromSteam(const std::string& steam64, const std::string& displayName) {
    const std::string uuid = PlayerUuidFromSteam64(steam64);

    std::unique_ptr<ProfileData> pd = PlayerDatabase::Get().GetField<ProfileData>(FieldKey::PROFILE_DATA, uuid);
    pd->set_playerid(uuid);
    DisplayName* dn = pd->mutable_displayname();
    dn->set_displayname(displayName.empty() ? "Player" : displayName);
    std::array<char, 5> disc{};
    std::mt19937 rng{std::random_device{}()};
    std::snprintf(disc.data(), 5, "%04d", std::uniform_int_distribution<int>(0, 9999)(rng)); // NOLINT
    dn->set_discriminator(disc.data());
    PlayerDatabase::Get().SetField(FieldKey::PROFILE_DATA, pd.get(), uuid);
    std::unique_ptr<PlayerData> pdata = PlayerDatabase::Get().GetField<PlayerData>(FieldKey::PLAYER_DATA, uuid);
    pdata->set_playerid(uuid);
    pdata->mutable_matchmakingdata()->set_playerid(uuid);
    PlayerDatabase::Get().SetField(FieldKey::PLAYER_DATA, pdata.get(), uuid);
    std::unique_ptr<OutfitLoadouts> outfits = PlayerDatabase::Get().GetField<OutfitLoadouts>(
        FieldKey::PLAYER_OUTFIT_LOADOUT, uuid);
    for (int i = 0; i < outfits->loadouts_size(); i++) {
        outfits->mutable_loadouts(i)->set_playerid(uuid);
    }
    PlayerDatabase::Get().SetField(FieldKey::PLAYER_OUTFIT_LOADOUT, outfits.get(), uuid);
    std::unique_ptr<WeaponLoadouts> weapons = PlayerDatabase::Get().GetField<WeaponLoadouts>(
        FieldKey::PLAYER_WEAPON_LOADOUT, uuid);
    for (int i = 0; i < weapons->weaponloadoutdata_size(); i++) {
        weapons->mutable_weaponloadoutdata(i)->set_playerid(uuid);
    }
    PlayerDatabase::Get().SetField(FieldKey::PLAYER_WEAPON_LOADOUT, weapons.get(), uuid);
    return uuid;
}

static const std::string& GetPragmaPrivateKey() {
    static std::string pragmaPrivateKey = [] {
        std::ifstream keyFile(ResourcesUtilities::GetResourcesFolder() / "pragma_private.pem");
        std::stringstream ss;
        ss << keyFile.rdbuf();
        return ss.str();
    }();
    return pragmaPrivateKey;
}

std::string AuthenticateHandler::BuildJwt(
    const std::string& backendType,
    const std::string& playerId,
    const std::string& socialId,
    const std::string& displayName,
    const std::string& discriminator) {
    const auto now = static_cast<int64_t>(time(nullptr));
    const auto exp = now + static_cast<int64_t>(24 * 3600); // 24 hrs

    picojson::object header = {
        {"kid", picojson::value("d3JtOq6jy3_HquwTsrzt81wh3BLiA-4f-qM8mj-0-YQ=")},
        {"alg", picojson::value("RS256")},
        {"typ", picojson::value("JWT")}};

    const std::string jti = boost::uuids::to_string(boost::uuids::random_generator()());
    picojson::object payload = {
        {"iss", picojson::value("pragma")},
        {"sub", picojson::value(backendType == "GAME" ? playerId : socialId)},
        {"iat", picojson::value(static_cast<double>(now))},
        {"exp", picojson::value(static_cast<double>(exp))},
        {"jti", picojson::value(jti)},
        {"sessionType", picojson::value("PLAYER")},
        {"backendType", picojson::value(backendType)},
        {"displayName", picojson::value(displayName)},
        {"discriminator", picojson::value(discriminator)},
        {"pragmaSocialId", picojson::value(socialId)},
        {"idProvider", picojson::value("STEAM")},
        {"extSessionInfo", picojson::value(R"({"permissions":0,"accountTags":["canary"]})")},
        {"expiresInMillis", picojson::value("86400000")},
        {"refreshInMillis", picojson::value("36203000")},
        {"pragmaPlayerId", picojson::value(playerId)}};

    if (backendType == "GAME") {
        payload["gameShardId"] = picojson::value("00000000-0000-0000-0000-000000000001");
    }

    auto token = jwt::create()
                     .set_header_claim("kid", jwt::claim(header["kid"]))
                     .set_header_claim("typ", jwt::claim(header["typ"]))
                     .set_header_claim("alg", jwt::claim(header["alg"]));
    for (auto& [key, value] : payload) {
        token.set_payload_claim(key, jwt::claim(value));
    }
    return token.sign(jwt::algorithm::rs256(
        "",
        GetPragmaPrivateKey(),
        "",
        ""));
}
