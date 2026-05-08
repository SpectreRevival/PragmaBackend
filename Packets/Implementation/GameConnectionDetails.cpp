#include <GameConnectionDetails.h>

#include <cstdlib>
#include <google/protobuf/util/json_util.h>
#include <spdlog/spdlog.h>
#include <string>

namespace pbuf = google::protobuf;

constexpr const char* kDefaultGameInstanceId = "185206f8-dae2-4144-923d-9ac326240f30";
constexpr const char* kDefaultHostname = "127.0.0.1";
constexpr int kDefaultPort = 7777;
constexpr const char* kDefaultMatchData = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
constexpr const char* kDefaultConnectionToken = "8656593e-1d5c-4a94-b460-190f7b694c95";

static std::string GetEnvString(const char* name, const char* fallback) {
    const char* value = std::getenv(name);
    if (value == nullptr || *value == '\0') {
        return fallback;
    }
    return value;
}

static int GetEnvInt(const char* name, int fallback) {
    const char* value = std::getenv(name);
    if (value == nullptr || *value == '\0') {
        return fallback;
    }
    try {
        return std::stoi(value);
    } catch (...) {
        return fallback;
    }
}

static std::string SerializeMessageToJson(const pbuf::Message& msg) {
    static const pbuf::util::JsonPrintOptions opts = []() {
        pbuf::util::JsonPrintOptions o;
        o.always_print_fields_with_no_presence = true;
        return o;
    }();
    std::string out;
    if (!pbuf::util::MessageToJsonString(msg, &out, opts).ok()) {
        spdlog::error("failed to serialize {} to json", msg.GetTypeName());
        return "{}";
    }
    return out;
}

GameConnectionDetails BuildGameConnectionDetailsFromEnv() {
    GameConnectionDetails details;
    details.set_gameinstanceid(GetEnvString("SPECTRE_GAME_INSTANCE_ID", kDefaultGameInstanceId));
    details.set_hostname(GetEnvString("SPECTRE_GAME_HOST", kDefaultHostname));
    details.set_port(GetEnvInt("SPECTRE_GAME_PORT", kDefaultPort));
    details.mutable_extconnectiondetails()->set_matchdata(GetEnvString("SPECTRE_GAME_MATCH_DATA", kDefaultMatchData));
    details.set_connectiontoken(GetEnvString("SPECTRE_GAME_CONNECTION_TOKEN", kDefaultConnectionToken));
    return details;
}

std::string SerializeHostConnectionDetailsResponse(const GameConnectionDetails& details) {
    GetHostConnectionDetailsV1Response wrapper;
    wrapper.mutable_hostconnectiondetails()->CopyFrom(details);
    return SerializeMessageToJson(wrapper);
}

std::string SerializeHostConnectionDetailsNotification(const GameConnectionDetails& details) {
    HostConnectionDetailsV1Notification wrapper;
    wrapper.mutable_connectiondetails()->CopyFrom(details);
    return SerializeMessageToJson(wrapper);
}

std::string SerializeAddedToGameNotification(const GameConnectionDetails& details) {
    AddedToGameV1Notification wrapper;
    wrapper.set_gameinstanceid(details.gameinstanceid());
    wrapper.mutable_ext()->set_matchleader(true);
    return SerializeMessageToJson(wrapper);
}
