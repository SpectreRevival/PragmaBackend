#include <SteamValidator.h>
#include <boost/asio.hpp>
#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>
#include <chrono>
#include <spdlog/spdlog.h>

using tcp = boost::asio::ip::tcp;
namespace http = boost::beast::http;
using json = nlohmann::json;

SteamValidator::SteamValidator(std::string apiKey)
    : apiKey(std::move(apiKey)) {}

std::string SteamValidator::HttpGet(const std::string& host, const std::string& target) {
    boost::asio::io_context ioc;
    tcp::resolver resolver{ioc};

    boost::beast::tcp_stream stream{ioc};
    const auto results = resolver.resolve(host, "80");
    stream.expires_after(std::chrono::seconds(3));
    stream.connect(results);
    http::request<http::string_body> req{http::verb::get, target, 11};
    req.set(http::field::host, host);
    stream.expires_after(std::chrono::seconds(3));
    http::write(stream, req);
    boost::beast::flat_buffer buffer;
    http::response<http::string_body> res;
    stream.expires_after(std::chrono::seconds(3));
    http::read(stream, buffer, res);
    boost::system::error_code ec;
    stream.socket().shutdown(tcp::socket::shutdown_both, ec); // NOLINT
    return res.body();
}

std::optional<SteamPlayerInfo> SteamValidator::ValidateSteamId(const std::string& steam64) const {
    if (apiKey.empty()) return std::nullopt;

    try {
        auto body = HttpGet("api.steampowered.com",
                            "/ISteamUser/GetPlayerSummaries/v2/?key=" + apiKey + "&steamids=" + steam64 /* + "&format=json"*/);
        auto j = json::parse(body);
        auto& players = j.at("response").at("players");

        if (players.empty()) return std::nullopt;
        SteamPlayerInfo out;
        auto& player = players.at(0);
        out.steamId = player.value("steamid", "");
        out.personaName = player.value("personaname", "");
        return out;
    } catch (const std::exception& e) {
        spdlog::warn("Steam API validation failed: {}", e.what());
        return std::nullopt;
    }
}
