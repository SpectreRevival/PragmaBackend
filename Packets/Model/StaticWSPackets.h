#pragma once
#include "Regex.h"
#include "SpectreRpcType.h"

void RegisterStaticHandlerFromFile(std::string filename, SpectreRpcType rpcType);
void RegisterRegexHandlerFromFiles(SpectreRpcType rpcType, std::initializer_list<std::pair<Regex, std::string>> map);
void RegisterStaticWSHandlers();