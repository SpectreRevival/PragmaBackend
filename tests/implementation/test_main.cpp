#include "SequencedRequestTest.h"

#include <HTTPRequestTest.h>
#include <ResourcesUtilities.h>
#include <WebsocketRequestTest.h>
#include <filesystem>
#include <gtest/gtest.h>

static std::vector<fs::path> gWsPaths = [] {
    std::vector<fs::path> paths;
    fs::path wsPath = ResourcesUtilities::GetResourcesFolder() / "testrequests" / "ws";
    if (fs::exists(wsPath) && fs::is_directory(wsPath)) {
        for (auto& item : std::filesystem::directory_iterator(wsPath)) {
            if (item.is_regular_file() && item.path().extension() == ".json") {
                paths.push_back(item.path());
            }
        }
    }
    return paths;
}();

static std::vector<fs::path> gHttpPaths = [] {
    std::vector<fs::path> paths;
    fs::path httpPath = ResourcesUtilities::GetResourcesFolder() / "testrequests" / "http";
    if (fs::exists(httpPath) && fs::is_directory(httpPath)) {
        for (auto& item : std::filesystem::directory_iterator(httpPath)) {
            if (item.is_regular_file() && item.path().extension() == ".json") {
                paths.push_back(item.path());
            }
        }
    }
    return paths;
}();

static std::vector<fs::path> gSequencedDirs = [] {
    std::vector<fs::path> sequencedDirs;
    fs::path sequencedPath = ResourcesUtilities::GetResourcesFolder() / "testrequests" / "sequenced";
    if (fs::exists(sequencedPath) && fs::is_directory(sequencedPath)) {
        for (auto& item : std::filesystem::directory_iterator(sequencedPath)) {
            if (item.is_directory()) {
                sequencedDirs.emplace_back(item.path().string());
            }
        }
    }
    return sequencedDirs;
}();
GTEST_ALLOW_UNINSTANTIATED_PARAMETERIZED_TEST(SequencedRequestTest);

INSTANTIATE_TEST_SUITE_P(WebsocketRequestTests, WebsocketRequestTest, ::testing::ValuesIn(gWsPaths));
INSTANTIATE_TEST_SUITE_P(HttpRequestTests, HTTPRequestTest, ::testing::ValuesIn(gHttpPaths));
INSTANTIATE_TEST_SUITE_P(SequencedRequestTests, SequencedRequestTest, ::testing::ValuesIn(gSequencedDirs));

int main(int argc, char** argv) {
    // Wait for the server to start up
    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}
