#include <BasicDatabase.h>
#include <utility>

BasicDatabase::BasicDatabase(const fs::path& dbPath, std::string tableName)
    : filename(dbPath), dbRaw(dbPath.string(), sql::OPEN_READWRITE | sql::OPEN_CREATE),
      tableName(std::move(tableName)) {
}

sql::Database* BasicDatabase::GetRaw() {
    return &dbRaw;
}

sql::Database& BasicDatabase::GetRawRef() {
    return dbRaw;
}

const std::string& BasicDatabase::GetTableName() {
    return tableName;
}