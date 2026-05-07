#pragma once

#include <nlohmann/json.hpp>
#include <string>

class GameConnectionDetails {
  public:
    std::string gameInstanceId;
    std::string hostname;
    int port = 7777;
    std::string matchData;
    std::string connectionToken;

    static GameConnectionDetails FromEnvironment();
    nlohmann::json ToHostConnectionDetailsPayload() const;
    nlohmann::json ToHostConnectionDetailsNotificationPayload() const;
    nlohmann::json ToAddedToGameNotificationPayload() const;
};
