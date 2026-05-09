#include "PersistenceUtilities.h"

#include <BackendEnvironment.h>
#include <ResourcesUtilities.h>
#include <filesystem>
#include <process.hpp>
#include <thread>

namespace fs = std::filesystem;

void BackendEnvironment::SetUp() {
#if defined(_WIN32)
    std::system("taskkill /f /im pragmabackend.exe 2>nul"); // NOLINT
#elif defined(__linux__)
    std::system("pkill -9 pragmabackend 2>/dev/null"); // NOLINT
#endif

    std::this_thread::sleep_for(std::chrono::milliseconds(500));

    std::filesystem::remove(PersistenceUtilities::GetSavePath() / "playerdata.sqlite");

    std::filesystem::path backendDir = ResourcesUtilities::GetCurrentExecutablePath().parent_path().parent_path();
    std::filesystem::path lockPath = backendDir / "server.lock";
    std::filesystem::remove(lockPath);

    std::filesystem::path exePath = backendDir / "pragmabackend";
#if defined(_WIN32)
    exePath.replace_extension(".exe");
#endif

    if (!fs::exists(exePath)) {
        throw std::runtime_error("Backend executable not found at: " + exePath.string());
    }

    std::string cmdLine = exePath.string() + " 8081 8082 8083";

    backendProcess = std::make_unique<TinyProcessLib::Process>(
        cmdLine,
        backendDir.string(),
        [](const char* bytes, size_t n) { /* stdout */ },
        [](const char* bytes, size_t n) { /* stderr */ });

    const auto maxWaitTime = std::chrono::seconds(60);
    const auto startTime = std::chrono::steady_clock::now();

    while (!fs::exists(lockPath)) {
        auto elapsed = std::chrono::steady_clock::now() - startTime;
        if (elapsed > maxWaitTime) {
            throw std::runtime_error(
                "Backend process failed to start within timeout. "
                "Expected lock file at: " +
                lockPath.string());
        }
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }
}

void BackendEnvironment::TearDown() {
    if (backendProcess) {
#if defined(_WIN32)
        std::system("taskkill /f /im pragmabackend.exe 2>nul"); // NOLINT
#elif defined(__linux__)
        std::system("pkill -9 pragmabackend 2>/dev/null"); // NOLINT
#endif

        std::this_thread::sleep_for(std::chrono::milliseconds(500));
        backendProcess.reset();
    }

    std::this_thread::sleep_for(std::chrono::milliseconds(500));

    std::filesystem::remove(PersistenceUtilities::GetSavePath() / "playerdata.sqlite");
    std::filesystem::remove(ResourcesUtilities::GetCurrentExecutablePath().parent_path().parent_path() / "server.lock");
}