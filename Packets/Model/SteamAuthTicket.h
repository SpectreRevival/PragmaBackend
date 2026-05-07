#pragma once
#include <string>

class SteamAuthTicket {
  public:
    static std::string ExtractSteamId64(const std::string& providerToken);
};