#pragma once
#include "ProtobufDatabase.h"

class PlayerDatabase : public ProtobufDatabase {
  public:
    static PlayerDatabase& Get();
    explicit PlayerDatabase(const fs::path& path);
};