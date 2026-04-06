#include "ResourcesUtilities.h"

#include <HTTPRequestTest.h>
#include <JsonTestUtil.h>
#include <TestHTTPClient.h>
#include <boost/beast/http.hpp>
#include <fstream>

void RunHTTPTest(const fs::path& testPath, json& outResponse) {
    std::ifstream testFile(testPath);
    std::stringstream ss;
    ss << testFile.rdbuf();
    std::string testJsonStr = ss.str();
    json testJson = json::parse(testJsonStr);
    return RunHTTPTest(testJson, outResponse);
}

std::vector<std::string> skipRoutes = []() {
    std::vector<std::string> skipRoutes;
    std::ifstream skipRoutesFile(ResourcesUtilities::GetResourcesFolder() / "testrequests" / "httpSkipTests.txt");
    std::string line;
    while (std::getline(skipRoutesFile, line)) {
        skipRoutes.push_back(line);
    }
    return skipRoutes;
}();

void RunHTTPTest(json testJson, json& outResponse) {
    if (std::ranges::find(skipRoutes, testJson.at("path").get<std::string>()) != skipRoutes.end()) {
        GTEST_SKIP() << "Route " << testJson.at("path") << "is in list of skip routes";
    }
    std::cout << "Test info: " << '\n';
    std::cout << "Path: " << testJson["path"].dump() << '\n';
    std::cout << "method: " << testJson["method"].dump() << '\n';
    std::cout << "request payload: " << testJson["request"].dump() << '\n';
    std::cout << "response payload: " << testJson["response"].dump() << '\n';
    boost::beast::http::response<boost::beast::http::string_body> res;
    if (testJson["method"] == "GET") {
        res = HTTPFetch(8081, testJson["path"], testJson["request"], boost::beast::http::verb::get);
    } else if (testJson["method"] == "POST") {
        res = HTTPFetch(8081, testJson["path"], testJson["request"], boost::beast::http::verb::post);
    } else {
        std::cerr << "Unrecognized http verb: " << testJson["method"] << '\n';
        GTEST_FATAL_FAILURE_("Unrecognized http verb");
    }
    if (res.body().empty()) {
        GTEST_FAIL() << "Got an empty response";
    }
    outResponse = json::parse(res.body());
    json expectedResponse = json::parse(testJson["response"].get<std::string>());
    EXPECT_TRUE(JsonMatchesSchema(outResponse, expectedResponse,
                                  testJson.contains("ignoreReplace") && testJson["ignoreReplace"] == true,
                                  !testJson.contains("ignoreAdd") || testJson["ignoreAdd"] == true));
}

TEST_P(HTTPRequestTest, ResponseValidation) {
    json out;
    RunHTTPTest(GetParam(), out);
}