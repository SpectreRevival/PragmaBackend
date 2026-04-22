#pragma once
#include "ProtobufDatabase.h"

class PlayerDatabase : public ProtobufDatabase {
  private:
    static PlayerDatabase inst;

  public:
    static PlayerDatabase& Get();
    explicit PlayerDatabase(const fs::path& path);
};