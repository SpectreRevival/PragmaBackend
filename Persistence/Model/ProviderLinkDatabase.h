#pragma once
#include "ProtobufDatabase.h"

#include <AuthProvider.h>
#include <string>

class ProviderLinkDatabase : public BasicDatabase {
private:
    static ProviderLinkDatabase inst;
public:
    static ProviderLinkDatabase& Get();
    ProviderLinkDatabase(const fs::path& dbPath);
    std::string LookupPlayerByProvider(AuthProvider provider, const std::string& providerId);
    std::string GetProviderIdByPlayerId(const std::string& playerId, AuthProvider provider);
    void UpsertProviderMap(AuthProvider provider, const std::string& providerId, const std::string& playerId);
};