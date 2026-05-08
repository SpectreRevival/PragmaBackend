#pragma once

#include <GameConnectionDetails.pb.h>
#include <string>

GameConnectionDetails BuildGameConnectionDetailsFromEnv();

std::string SerializeHostConnectionDetailsResponse(const GameConnectionDetails& details);
std::string SerializeHostConnectionDetailsNotification(const GameConnectionDetails& details);
std::string SerializeAddedToGameNotification(const GameConnectionDetails& details);
