#include "ProtobufDatabase.h"

#include <spdlog/spdlog.h>
#include <utility>

std::unordered_map<FieldKey, ProtobufDatabaseFieldData> ProtobufDatabase::fieldData{};

ProtobufDatabase::ProtobufDatabase(const fs::path& dbPath, std::string tableName, std::string keyFieldName, const std::string& /*keyFieldType*/)
    : BasicDatabase(dbPath, tableName), keyFieldName(std::move(keyFieldName)) {
    GetRaw()->exec("CREATE TABLE IF NOT EXISTS " + GetTableName() + " (" + GetKeyFieldName() + " " + GetKeyFieldType() + " PRIMARY KEY);");
}

sql::Statement ProtobufDatabase::FormatStatement(std::string command, FieldKey key) {
    size_t tablePos = command.find("{table}");
    while (tablePos != std::string::npos) {
        command.replace(tablePos, sizeof("{table}") - 1, GetTableName());
        tablePos = command.find("{table}");
    }
    size_t colPos = command.find("{col}");
    while (colPos != std::string::npos) {
        command.replace(colPos, sizeof("{col}") - 1, GetFieldName(key));
        colPos = command.find("{col}");
    }
    return sql::Statement(*GetRaw(), command);
}

/** Makes the following assumptions:
* - statement is valid SQL
* - statement has 1 questionmark, and it's index(1-indexed) is passed for dataBindIndex
Eg valid: INSERT INTO players (PlayerName) VALUES (?) WHERE HI=? B=2
*/
void ProtobufDatabase::SetField(sql::Statement& statement, FieldKey key, const pbuf::Message* object, const uint32_t dataBindIndex) {
    if (dataBindIndex == 0) {
        spdlog::error("passed 0 for data bind index which is not valid");
        throw;
    }
    try {
        std::vector<uint8_t> bytes(object->ByteSizeLong() + sizeof(FieldKey));
        object->SerializeToArray(bytes.data() + sizeof(FieldKey), object->ByteSizeLong());
        memcpy(bytes.data(), &key, sizeof(FieldKey));
        statement.bind(dataBindIndex, bytes.data(), bytes.size());
        statement.exec();
    } catch (const std::exception& e) {
        spdlog::error("Failed to set field with FieldKey {}: {}", fieldData.at(key).GetFieldName(), e.what());
        throw;
    }
}

void ProtobufDatabase::SetField(FieldKey key, const pbuf::Message* object, const std::string& dbKeyId) {
    sql::Statement setStatement = FormatStatement(
        "INSERT INTO {table} (" + GetKeyFieldName() + ", {col}) VALUES(?,?) ON CONFLICT(" + GetKeyFieldName() + ") DO UPDATE SET {col} = excluded.{col};",
        key);
    setStatement.bind(1, dbKeyId);
    SetField(setStatement, key, object, 2);
}

bool ProtobufDatabase::IsFieldPopulated(FieldKey key, const std::string& dbKey) {
    sql::Statement query = FormatStatement(
        "SELECT {col} FROM {table} WHERE " + GetKeyFieldName() + " = ? COLLATE NOCASE",
        key);
    query.bind(1, dbKey);
    return query.executeStep();
}

const std::string& ProtobufDatabase::GetFieldName(FieldKey key) {
    return fieldData.at(key).GetFieldName();
}

const std::string& ProtobufDatabase::GetKeyFieldName() {
    return keyFieldName;
}

const std::string& ProtobufDatabase::GetKeyFieldType() {
    return keyFieldType;
}