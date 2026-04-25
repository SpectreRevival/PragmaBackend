#include "PersistenceUtilities.h"

#include <BackendEnvironment.h>
#include <ResourcesUtilities.h>
#include <filesystem>
#include <thread>
#define WIN32_LEAN_AND_MEAN // why is this a thing. it literally won't compile w/o it
#include <windows.h>

namespace fs = std::filesystem;

static DWORD RunBackendWindows() {
    STARTUPINFOA si = {sizeof(si)};
    PROCESS_INFORMATION pi = {};
    std::filesystem::path exePath = ResourcesUtilities::GetCurrentExecutablePath().parent_path().parent_path() /
                                    "pragmabackend.exe";
    BOOL ok = CreateProcessA(exePath.string().c_str(),
        (LPSTR)"8081 8082 8083",
        nullptr,
        nullptr,
        TRUE,
        CREATE_NEW_PROCESS_GROUP,
        nullptr,
        nullptr,
        &si,
        &pi);
    FreeConsole();
    FreeConsole();
    for (int i = 0; i < 50; i++) {
        if (AttachConsole(pi.dwProcessId) != 0) break;
        std::this_thread::sleep_for(std::chrono::milliseconds(50));
    }
    SetConsoleCtrlHandler(nullptr, TRUE);
    return pi.dwProcessId;
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
    std::filesystem::remove(PersistenceUtilities::GetSavePath() / "playerdata.sqlite");
    backendPID = RunBackendWindows();
    while (true) {
        if (fs::exists(ResourcesUtilities::GetCurrentExecutablePath().parent_path().parent_path() / "server.lock")) {
            break;
        }
    }
}

void BackendEnvironment::TearDown() {
    GenerateConsoleCtrlEvent(CTRL_C_EVENT, NULL);
    SetConsoleCtrlHandler(nullptr, FALSE);
    HANDLE h = OpenProcess(SYNCHRONIZE, FALSE, backendPID);
    WaitForSingleObject(h, INFINITE);
    CloseHandle(h);
    std::this_thread::sleep_for(std::chrono::seconds(1)); // wait for windows to release file handle
    std::filesystem::remove(PersistenceUtilities::GetSavePath() / "playerdata.sqlite");
    std::filesystem::remove(ResourcesUtilities::GetCurrentExecutablePath().parent_path().parent_path() / "server.lock");
}