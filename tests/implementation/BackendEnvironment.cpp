#include "AuthLatch.h"
#include "BanDatabase.h"
#include "PartyDatabase.h"
#include "PersistenceUtilities.h"
#include "PlayerDatabase.h"
#include "ProviderLinkDatabase.h"

#include <BackendEnvironment.h>
#include <ResourcesUtilities.h>
#include <ServerMainThread.h>
#include <filesystem>
#include <fstream>
#include <thread>

namespace fs = std::filesystem;

void BackendEnvironment::CleanStoredInformation() {
    PlayerDatabase::Get().GetRaw()->exec("DELETE FROM " + PlayerDatabase::Get().GetTableName());
    PartyDatabase::Get().GetRaw()->exec("DELETE FROM " + PartyDatabase::Get().GetTableName());
    BanDatabase::Get().GetRaw()->exec("DELETE FROM " + BanDatabase::Get().GetTableName());
    ProviderLinkDatabase::Get().GetRaw()->exec("DELETE FROM " + ProviderLinkDatabase::Get().GetTableName());
    AuthLatch::Get().Clear();
}

void BackendEnvironment::SetUp() {
#if defined(_WIN32)
    std::filesystem::path exePath = ResourcesUtilities::GetCurrentExecutablePath().parent_path().parent_path() /
                                    "pragmabackend.exe";
    std::system("taskkill /f /im pragmabackend.exe"); // NOLINT
#elif defined(__linux__)
    std::filesystem::path exePath = ResourcesUtilities::GetCurrentExecutablePath().parent_path().parent_path() /
                                    "pragmabackend";
    std::system("pkill -9 pragmabackend"); // NOLINT
#endif
    std::this_thread::sleep_for(std::chrono::milliseconds(500));
    CleanStoredInformation();
}

void BackendEnvironment::TearDown() {
    CleanStoredInformation();
}