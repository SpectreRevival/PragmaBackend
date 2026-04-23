#include "PersistenceUtilities.h"

#include <BackendEnvironment.h>
#include <ResourcesUtilities.h>
#include <filesystem>
#include <thread>
#include <fstream>
#include <ServerMainThread.h>

namespace fs = std::filesystem;

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
    // Wait for file handle release in case that there was a backend instance already running
    std::this_thread::sleep_for(std::chrono::milliseconds(500));
    std::filesystem::remove(PersistenceUtilities::GetSavePath() / "playerdata.sqlite");
    static std::vector<std::string> argStrings{
        ResourcesUtilities::GetCurrentExecutablePath().string(),
        "8081",
        "8082",
        "8083"
    };
    static std::vector<char*> args{
        argStrings[0].data(),
        argStrings[1].data(),
        argStrings[2].data(),
        argStrings[3].data()
    };
    server = std::jthread([](std::stop_token st) {
        MainThread(4, args.data(), st);
    });
    while (true) {
        if (serverOnline) {
            std::this_thread::sleep_for(std::chrono::milliseconds(500));
            break;
        }
        std::this_thread::sleep_for(std::chrono::milliseconds(50));
    }
}

void BackendEnvironment::TearDown() {
    if (server.joinable()) {
        server.request_stop();
        server.join();
        std::this_thread::sleep_for(std::chrono::seconds(1)); // wait for windows to release file handle
    }
    std::filesystem::remove(PersistenceUtilities::GetSavePath() / "playerdata.sqlite");
    std::filesystem::remove(ResourcesUtilities::GetCurrentExecutablePath().parent_path().parent_path() / "server.lock");
}