#pragma once
#include "BasicDatabase.h"
#include "CaseHelper.h"
#include "FieldKey.h"
#include "ProtobufDatabaseFieldData.h"

#include <SQLiteCpp/SQLiteCpp.h>
#include <filesystem>
#include <fstream>
#include <google/protobuf/message.h>
#include <google/protobuf/util/json_util.h>
#include <spdlog/spdlog.h>
#include <sstream>
#include <type_traits>
#include <unordered_map>
#include <vector>

namespace fs = std::filesystem;
namespace pbuf = google::protobuf;
namespace sql = SQLite;

// Template implementations have to be in the headers to not get linker errors, :C it's a bit ugly
/* Sample code:
    PlayerDatabase playerData(fs::path("playerdata.sqlite"));
    playerData.AddPrototype<PlayerName>(FieldKey::PLAYER_INGAME_NAME);
    PlayerName name;
    name.set_name("myname");
    sql::Statement statement = playerData.FormatStatement(
        "INSERT INTO {table} ({col}) VALUES(?)", FieldKey::PLAYER_INGAME_NAME
    );
    playerData.SetField(statement, FieldKey::PLAYER_INGAME_NAME, &name);
    sql::Statement fetchStatement = playerData.FormatStatement(
        "SELECT {col} FROM {table} LIMIT 1", FieldKey::PLAYER_INGAME_NAME
    );
    std::unique_ptr<PlayerName> nameFetched = playerData.GetField<PlayerName>(fetchStatement, FieldKey::PLAYER_INGAME_NAME);
    spdlog::info("name: {}", nameFetched->name());
*/
class ProtobufDatabase : public BasicDatabase {
  private:
    std::string keyFieldName;
    std::string keyFieldType;
    pbuf::util::JsonParseOptions parseOpts;
    static std::unordered_map<FieldKey, ProtobufDatabaseFieldData>& getFieldData() {
        static std::unordered_map<FieldKey, ProtobufDatabaseFieldData> instance;
        return instance;
    }

    template <typename T>
    std::unique_ptr<T> DefaultOrNullptr(FieldKey key) {
        static_assert(std::is_base_of_v<pbuf::Message, T>, "Type provided to DefaultOrNullptr must inherit from protobuf::Message");
        if (getFieldData().at(key).GetDefaultValue() == nullptr) {
            return nullptr;
        }
        spdlog::info("Returning default value for FieldKey: {}", static_cast<uint32_t>(key));
        const T* typed = dynamic_cast<const T*>(getFieldData().at(key).GetDefaultValue());
        if (!typed) {
            // Handle type mismatch
            spdlog::error("type mismatch in DefaultOrNullptr");
            return nullptr;
        }

        auto copy = std::make_unique<T>();
        copy->CopyFrom(*typed);
        return std::move(copy);
    }

  public:
    ProtobufDatabase(const fs::path& dbPath, const std::string& tableName, std::string keyFieldName, const std::string& keyFieldType);

    template <typename T>
    std::vector<std::unique_ptr<T>> GetFields(sql::Statement& query, FieldKey key) {
        static_assert(std::is_base_of_v<pbuf::Message, T>, "Type provided to GetFields must inherit from protobuf::Message");
        std::vector<std::unique_ptr<T>> output;
        while (query.executeStep()) {
            if (query.getColumnCount() != 1) {
                spdlog::warn("Multiple columns returned by query passed into GetFields w FieldKey {}, ignoring all columns except first one", getFieldData().at(key).GetFieldName());
            }
            const char* blob = static_cast<const char*>(query.getColumn(0).getBlob());
            int sz = query.getColumn(0).getBytes();
            if (sz < sizeof(FieldKey)) {
                spdlog::error("sz < {} in GetFields, cell cannot contain a FieldKey", sizeof(FieldKey));
                throw;
            }
            FieldKey savedFieldKey{};
            memcpy(&savedFieldKey, blob, sizeof(FieldKey));
            if (savedFieldKey != key) {
                spdlog::error("FieldKey passed to GetFields {} was not the same as FieldKey found in saved object", static_cast<uint32_t>(key));
                throw;
            }
            std::unique_ptr<T> object = std::make_unique<T>();
            if (!object->ParseFromArray(blob + sizeof(FieldKey), sz - sizeof(FieldKey))) {
                spdlog::error("parse failure");
                throw;
            }
            output.push_back(std::move(object));
        }
        return output;
    }

    template <typename T>
    std::unique_ptr<T> GetField(sql::Statement& query, FieldKey key) {
        static_assert(std::is_base_of_v<pbuf::Message, T>, "Type provided to GetField must inherit from protobuf::Message");
        if (!query.executeStep()) {
            return std::move(DefaultOrNullptr<T>(key));
        }
        if (query.getColumnCount() != 1) {
            spdlog::warn("Multiple columns returned by query passed into GetField w FieldKey {}, ignoring all columns except first one",
                         getFieldData().at(key).GetFieldName());
        }
        const char* blob = static_cast<const char*>(query.getColumn(0).getBlob());
        int sz = query.getColumn(0).getBytes();
        if (sz <= 0) {
            return std::move(DefaultOrNullptr<T>(key));
        }
        if (sz < sizeof(FieldKey)) {
            spdlog::error("sz < {} but > 0 in GetField, cell cannot contain a FieldKey", sizeof(FieldKey));
            return nullptr;
        }
        FieldKey savedFieldKey;
        memcpy(&savedFieldKey, blob, sizeof(FieldKey));
        if (savedFieldKey != key) {
            spdlog::error("FieldKey passed to GetFields {} was not the same as FieldKey found in saved object", static_cast<uint32_t>(key));
            return std::move(DefaultOrNullptr<T>(key));
        }
        std::unique_ptr<T> object = std::make_unique<T>();
        if (!object->ParseFromArray(blob + sizeof(FieldKey), sz - sizeof(FieldKey))) {
            spdlog::error("parse failure");
            return std::move(DefaultOrNullptr<T>(key));
        }
        return std::move(object);
    }

    template <typename T>
    std::unique_ptr<T> GetField(FieldKey key, const std::string& dbKey) {
        sql::Statement query = FormatStatement(
            "SELECT {col} FROM {table} WHERE " + GetKeyFieldName() + " = ? COLLATE NOCASE",
            key);
        query.bind(1, dbKey);
        return std::move(GetField<T>(query, key));
    }

    sql::Statement FormatStatement(std::string command, FieldKey key);

    void AddPrototype(FieldKey key, ProtobufDatabaseFieldData dat) {
        sql::Statement colQuery(*GetRaw(), "PRAGMA table_info(" + GetTableName() + ");");
        bool colExists = false;
        while (colQuery.executeStep()) {
            std::string colName = colQuery.getColumn(1).getText();
            if (CaseInsensitiveEquals(colName, dat.GetFieldName())) {
                colExists = true;
                break;
            }
        }
        if (!colExists) {
            try {
                GetRaw()->exec("ALTER TABLE " + GetTableName() + " ADD COLUMN " + dat.GetFieldName() + " BLOB;");
            } catch (std::exception& e) {
                spdlog::error("{}", e.what());
            }
        }
        getFieldData().insert_or_assign(key, std::move(dat));
    }

    bool IsFieldPopulated(FieldKey key, const std::string& dbKey);

    static void SetField(sql::Statement& statement, FieldKey key, const pbuf::Message* object, uint32_t dataBindIndex);
    void SetField(FieldKey key, const pbuf::Message* object, const std::string& dbKeyId);

    static const std::string& GetFieldName(FieldKey key);

    const std::string& GetKeyFieldName();
    const std::string& GetKeyFieldType();
};
