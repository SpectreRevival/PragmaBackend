#include "SequencedRequestTest.h"
#include "ServerMainThread.h"

#include <HTTPRequestTest.h>
#include <ResourcesUtilities.h>
#include <WebsocketRequestTest.h>
#include <filesystem>
#include <gtest/gtest.h>

static std::vector<fs::path> gWsPaths = [] {
    std::vector<fs::path> paths;
    for (auto& item : std::filesystem::directory_iterator(ResourcesUtilities::GetResourcesFolder() / "testrequests" / "ws")) {
        if (item.is_regular_file() && item.path().extension() == ".json") {
            paths.push_back(item.path());
        }
    }
    return paths;
}();

static std::vector<fs::path> gHttpPaths = [] {
    std::vector<fs::path> paths;
    for (auto& item : std::filesystem::directory_iterator(ResourcesUtilities::GetResourcesFolder() / "testrequests" / "http")) {
        if (item.is_regular_file() && item.path().extension() == ".json") {
            paths.push_back(item.path());
        }
    }
    return paths;
}();

static std::vector<fs::path> gSequencedDirs = [] {
    std::vector<fs::path> sequencedDirs;
    for (auto& item : std::filesystem::directory_iterator(ResourcesUtilities::GetResourcesFolder() / "testrequests" / "sequenced")) {
        if (item.is_directory()) {
            sequencedDirs.emplace_back(item.path().string());
        }
    }
    return sequencedDirs;
}();

INSTANTIATE_TEST_SUITE_P(WebsocketRequestTests, WebsocketRequestTest, ::testing::ValuesIn(gWsPaths));
INSTANTIATE_TEST_SUITE_P(HttpRequestTests, HTTPRequestTest, ::testing::ValuesIn(gHttpPaths));
INSTANTIATE_TEST_SUITE_P(SequencedRequestTests, SequencedRequestTest, ::testing::ValuesIn(gSequencedDirs));

int main(int argc, char** argv) {
    // Wait for the server to start up
    ::testing::InitGoogleTest(&argc, argv);
    testing::UnitTest* unitTest = testing::UnitTest::GetInstance();
    std::jthread backendServerThread;
    if (unitTest->test_to_run_count() != 0) {
        static std::vector<std::string> argStrings{
            ResourcesUtilities::GetCurrentExecutablePath().string(),
            "8081",
            "8082",
            "8083"};
        static std::vector<char*> args{
            argStrings[0].data(),
            argStrings[1].data(),
            argStrings[2].data(),
            argStrings[3].data()};
        backendServerThread = std::jthread([](std::stop_token st) {
            MainThread(4, args.data(), st);
        });
        while (true) {
            if (serverOnline) {
                std::this_thread::sleep_for(std::chrono::milliseconds(500));
                break;
            }
            std::this_thread::sleep_for(std::chrono::milliseconds(50));
        }
        BackendEnvironment::CleanStoredInformation();
    }
    int exitCode = RUN_ALL_TESTS();
    if (backendServerThread.joinable()) {
        backendServerThread.request_stop();
        backendServerThread.join();
    }
    return exitCode;
}