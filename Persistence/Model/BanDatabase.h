#pragma once
#include "ProtobufDatabase.h"

class BanDatabase : public BasicDatabase {
  public:
    static BanDatabase& Get();
    BanDatabase(const fs::path& path);
    bool IsBanned(const std::string& playerId);
};