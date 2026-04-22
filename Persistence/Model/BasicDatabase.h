#pragma once

#include <SQLiteCpp/SQLiteCpp.h>
#include <filesystem>

namespace fs = std::filesystem;
namespace sql = SQLite;

class BasicDatabase {
  private:
    fs::path filename;
    sql::Database dbRaw;
    std::string tableName;

  public:
    BasicDatabase(const fs::path& dbPath, const std::string& tableName);
    sql::Database* GetRaw();
    sql::Database& GetRawRef();
    const std::string& GetTableName();
};