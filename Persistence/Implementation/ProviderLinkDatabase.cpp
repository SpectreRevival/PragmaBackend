#include "PersistenceUtilities.h"

#include <ProviderLinkDatabase.h>

ProviderLinkDatabase ProviderLinkDatabase::inst{PersistenceUtilities::GetSavePath() / "playerdata.sqlite"};

ProviderLinkDatabase& ProviderLinkDatabase::Get() {
    return inst;
}

ProviderLinkDatabase::ProviderLinkDatabase(const fs::path& dbPath)
    : BasicDatabase(dbPath, "providers") {
    GetRaw()->exec(
        "CREATE TABLE IF NOT EXISTS providers ("
        "provider TEXT NOT NULL,"
        "provider_id TEXT NOT NULL,"
        "player_id TEXT NOT NULL,"
        "PRIMARY KEY(provider, provider_id));");
    GetRaw()->exec("CREATE INDEX IF NOT EXISTS providers_player_idx ON providers(player_id);");
}

std::string ProviderLinkDatabase::LookupPlayerByProvider(AuthProvider provider, const std::string& providerId) {
    auto* raw = GetRaw();
    SQLite::Statement query(*raw, "SELECT player_id FROM providers WHERE provider=? AND provider_id=?;");
    query.bind(1, static_cast<int32_t>(provider));
    query.bind(2, providerId);
    if (query.executeStep()) return query.getColumn(0).getString();
    return {};
}

std::string ProviderLinkDatabase::GetProviderIdByPlayerId(const std::string& playerId, AuthProvider provider) {
    auto* raw = GetRaw();
    SQLite::Statement query(*raw, "SELECT provider_id FROM providers WHERE player_id=? AND provider=?;");
    query.bind(1, playerId);
    query.bind(2, static_cast<int32_t>(provider));
    if (query.executeStep()) {
        return query.getColumn(0).getString();
    }
    return "";
}

void ProviderLinkDatabase::UpsertProviderMap(AuthProvider provider, const std::string& providerId, const std::string& playerId) {
    auto* raw = GetRaw();
    SQLite::Statement query(*raw, "INSERT OR REPLACE INTO providers(provider, provider_id, player_id) VALUES(?,?,?);");
    query.bind(1, static_cast<int32_t>(provider));
    query.bind(2, providerId);
    query.bind(3, playerId);
    query.exec();
}