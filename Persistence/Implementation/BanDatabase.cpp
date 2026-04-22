#include "PersistenceUtilities.h"

#include <BanDatabase.h>

BanDatabase BanDatabase::inst{PersistenceUtilities::GetSavePath() / "playerdata.sqlite"};

BanDatabase& BanDatabase::Get() {
    return inst;
}

BanDatabase::BanDatabase(const fs::path& path) : BasicDatabase(path, "bans") {
    GetRaw()->exec("CREATE TABLE IF NOT EXISTS bans("
            "player_id TEXT PRIMARY KEY,"
            "reason TEXT,"
            "banned_until INTEGER DEFAULT 0)");
    SQLite::Statement probe(*GetRaw(), "SELECT 1 FROM pragma_table_info('bans') WHERE name='banned_until';");
    if (!probe.executeStep()) {
        GetRaw()->exec("ALTER TABLE bans ADD COLUMN banned_until INTEGER DEFAULT 0;");
        GetRaw()->exec("UPDATE bans SET banned_until=0 WHERE banned_until IS NULL;");
    }
}


bool BanDatabase::IsBanned(const std::string& playerId) {
    auto* raw = GetRaw();
    SQLite::Statement query(*raw, "SELECT banned_until FROM bans WHERE player_id=?;");
    query.bind(1, playerId);
    if (!query.executeStep()) return false;
    int64_t untilMs = 0;
    if (!query.getColumn(0).isNull()) {
        untilMs = query.getColumn(0).getInt64();
    }

    if (untilMs == 0) return true;
    const auto nowMs = std::chrono::duration_cast<std::chrono::milliseconds>(
                           std::chrono::system_clock::now().time_since_epoch())
                           .count();
    return nowMs < untilMs;
}