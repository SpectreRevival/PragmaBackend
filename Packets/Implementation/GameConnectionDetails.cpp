#include <GameConnectionDetails.h>

#include <cstdlib>

namespace {
    constexpr const char* defaultGameInstanceId = "185206f8-dae2-4144-923d-9ac326240f30";
    constexpr const char* defaultHostname = "127.0.0.1";
    constexpr int defaultPort = 7777;
    constexpr const char* defaultMatchData = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
    constexpr const char* defaultConnectionToken = "8656593e-1d5c-4a94-b460-190f7b694c95";

    std::string GetEnvString(const char* name, const char* fallback) {
        const char* value = std::getenv(name);
        if (value == nullptr || *value == '\0') {
            return fallback;
        }
        return value;
    }

    int GetEnvInt(const char* name, int fallback) {
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
}

GameConnectionDetails GameConnectionDetails::FromEnvironment() {
    GameConnectionDetails details;
    details.gameInstanceId = GetEnvString("SPECTRE_GAME_INSTANCE_ID", defaultGameInstanceId);
    details.hostname = GetEnvString("SPECTRE_GAME_HOST", defaultHostname);
    details.port = GetEnvInt("SPECTRE_GAME_PORT", defaultPort);
    details.matchData = GetEnvString("SPECTRE_GAME_MATCH_DATA", defaultMatchData);
    details.connectionToken = GetEnvString("SPECTRE_GAME_CONNECTION_TOKEN", defaultConnectionToken);
    return details;
}

nlohmann::json GameConnectionDetails::ToHostConnectionDetailsPayload() const {
    nlohmann::json payload;
    payload["hostConnectionDetails"] = {
        {"gameInstanceId", gameInstanceId},
        {"extConnectionDetails", {{"matchData", matchData}}},
        {"hostname", hostname},
        {"port", port},
        {"connectionToken", connectionToken},
    };
    return payload;
}

nlohmann::json GameConnectionDetails::ToHostConnectionDetailsNotificationPayload() const {
    nlohmann::json payload;
    payload["connectionDetails"] = ToHostConnectionDetailsPayload()["hostConnectionDetails"];
    return payload;
}

nlohmann::json GameConnectionDetails::ToAddedToGameNotificationPayload() const {
    nlohmann::json payload;
    payload["gameInstanceId"] = gameInstanceId;
    payload["ext"] = {{"matchLeader", true}};
    return payload;
}
