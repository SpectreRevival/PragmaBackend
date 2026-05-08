#include <BasicDatabase.h>
#include <utility>

BasicDatabase::BasicDatabase(const fs::path& dbPath, std::string tableName)
    : filename(dbPath), dbRaw(dbPath.string(), sql::OPEN_READWRITE | sql::OPEN_CREATE),
      tableName(std::move(tableName)) {

    /*
        required because every db class opens its own connection
        to the same playerdata.sqlite. otherwise we'd get
        SQLITE_BUSY exceptions that throw out of request handlers :(
    */
    dbRaw.exec("PRAGMA journal_mode=WAL;");
    dbRaw.setBusyTimeout(5000);
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