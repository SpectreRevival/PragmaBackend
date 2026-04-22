#pragma once
#include <FieldKey.h>
#include <SQLiteCpp/Statement.h>
#include <filesystem>
#include <fstream>
#include <google/protobuf/message.h>
#include <google/protobuf/util/json_util.h>
#include <spdlog/spdlog.h>
#include <string>

class ProtobufDatabaseFieldData {
  private:
    FieldKey fieldKey;
    std::string fieldName;
    std::unique_ptr<google::protobuf::Message> defaultFieldValue;
    std::function<bool(google::protobuf::Message*)> updateHandler;
    static google::protobuf::util::JsonParseOptions parseOpts;

  public:
    ProtobufDatabaseFieldData(FieldKey fieldKey, std::string className,
                      std::unique_ptr<google::protobuf::Message> defaultFieldValue = nullptr,
                      std::function<bool(google::protobuf::Message*)> updateHandler = nullptr);

    template <typename T>
    static ProtobufDatabaseFieldData WithDefaultFromPath(FieldKey fieldKey, std::string className, const std::filesystem::path& defaultFieldValuePath,
                                                 std::function<bool(google::protobuf::Message*)> updateHandler = nullptr) {
        const std::ifstream defaultFile(defaultFieldValuePath);
        std::stringstream buf;
        buf << defaultFile.rdbuf();
        const std::string data = buf.str();
        std::unique_ptr<T> defaultFieldValue = std::make_unique<T>();
        if (auto status = google::protobuf::util::JsonStringToMessage(data, defaultFieldValue.get(), parseOpts); !status.ok()) {
            spdlog::error("failed to parse default message: {}", status.message());
            throw;
        }
        return ProtobufDatabaseFieldData(fieldKey, className, std::move(defaultFieldValue), updateHandler);
    }

    bool PreUpdateField(google::protobuf::Message* newData);
    [[nodiscard]] const FieldKey& GetFieldKey() const;
    [[nodiscard]] const std::string& GetFieldName() const;
    [[nodiscard]] const google::protobuf::Message* GetDefaultValue() const;
};