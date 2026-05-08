#include "google/protobuf/util/json_util.h"
#include "spdlog/spdlog.h"

#include <ProtobufDatabaseFieldData.h>
#include <fstream>
#include <utility>

ProtobufDatabaseFieldData::ProtobufDatabaseFieldData(FieldKey fieldKey, std::string className,
                                                     std::unique_ptr<google::protobuf::Message> defaultFieldValue,
                                                     std::function<bool(google::protobuf::Message*)> updateHandler)
    : fieldKey(fieldKey), fieldName(std::move(className)), defaultFieldValue(std::move(defaultFieldValue)), updateHandler(std::move(updateHandler)) {
}

google::protobuf::util::JsonParseOptions ProtobufDatabaseFieldData::parseOpts = {
    .allow_legacy_nonconformant_behavior = false,
    .ignore_unknown_fields = false,
    .case_insensitive_enum_parsing = true};

bool ProtobufDatabaseFieldData::PreUpdateField(google::protobuf::Message* newData) {
    if (updateHandler != nullptr) {
        return updateHandler(newData);
    }
    return true;
}

const std::string& ProtobufDatabaseFieldData::GetFieldName() const {
    return fieldName;
}

const google::protobuf::Message* ProtobufDatabaseFieldData::GetDefaultValue() const {
    return defaultFieldValue.get();
}

const FieldKey& ProtobufDatabaseFieldData::GetFieldKey() const {
    return fieldKey;
}