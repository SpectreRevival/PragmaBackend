#include "boost/asio/connect.hpp"
#include "nlohmann/json.hpp"

#include <TestHTTPClient.h>
#include <TestWebsocketClient.h>
#include <future>
#include <google/protobuf/util/json_util.h>

TestWebsocketClient::TestWebsocketClient(unsigned short port)
    : ioCtx(), nextRequestId(0), workGuard(boost::asio::make_work_guard(ioCtx)) {
    workerThread = std::thread([this]() { ioCtx.run(); });
    boost::asio::ip::tcp::resolver resolver(ioCtx);
    ws = std::make_shared<boost::beast::websocket::stream<boost::asio::ip::tcp::socket>>(ioCtx);
    HTTPFetch(8081, "/v1/submitproviderid", R"({"providerId": "76561199041068696"})", boost::beast::http::verb::post);
    boost::beast::http::response<boost::beast::http::string_body> authRes = HTTPFetch(8081, "/v1/account/authenticateorcreatev2", R"({
    "providerId": "STEAM",
    "providerToken": "14000000101B0A7F70C1FBBEE7E69F6C01001001B908BE67180000000100000002000000C1F9C3FB3D7764BEA0A8010001000000B20000003200000004000000E7E69F6C010010013E4E280061342A2D061C6E6400000000AD58BA672D08D66701007A730E00000000008561B7CAE2C32A312B14297D110EC706041E747242EC356EC06CC7CBED9E88B06ACE7C131B109603310CD4EBBD780C0CBD71DDFD43C7FDDB210DD69E69A76D7BE28F51C11FDEF5E04ACA018AB9333065E4340076F77F12BD906C57366CF6FD93931D52A9F0657279759865B5C60E99538904EBD23A3804FE02D594647943FE49",
    "gameShardId": "00000000-0000-0000-0000-000000000001",
    "loginQueuePassToken": "eyJraWQiOiJkM0p0T3E2ankzX0hxdXdUc3J6dDgxd2gzQkxpQS00Zi1xTThtai0wLVlRPSIsImFsZyI6IlJTMjU2IiwidHlwIjoiSldUIn0.eyJpc3MiOiJwcmFnbWEiLCJzdWIiOiIxMTYyOCIsImlhdCI6MTc0MDUwNzQyNywiZXhwIjoxNzQwNTkzODI3LCJqdGkiOiIyYjFiMzc5Yi1jZmQwLTRkMzAtYWE4NS02OWMyZWIyMWE4MWEiLCJ0aWNrZXROdW1iZXIiOiIxMTYyOCIsImlzQWxsb3dlZEluIjoidHJ1ZSJ9.PJp5MNXz2_xvCkq_XjzZeui1MvS9ylDLDgeLkJiv9jp_FVnTI9LISMtajHcef-7JehNs5sQC6P_Gpmb6JuVdD4k7HUX7a9IAgM8HKAagnfgmymn02SSpL7Mfz9wbH8FgOYU2ylKG_ExIW_aSG5HK588_waNeSydygwX2zRoSf8ZYZzbUHmMsZcG2iXpDq_Peejbt6Cgep9lsyNE5L5ZZzil9_KVu3FaEojcrI7tiPpHX7wi2K_J78rxmg2weUreowhv0VJA-YGqtOUlqFl7Ep8VGi-IrJdAf4gLeiVZMQoktc_g5tD9FgXzEAH_aDoBqGgoqnbKLcWLRiT1TAYGgXtCfw15Efh_ta-h4IIOI-DAnhJ1ujapd80Z87Wo6h7SpBaOitaI-bjBPkqDQGe2JooUNCrki848vPrfu0IQW00vawUtLX6LaS_aAEs0L2Vjxyebk1X37E9KwTDoxQGdmurutcnvSmVXOoO4P8F6o4oGx-A9d6HgFJl5rRie2LrWSJHlmcFm5_IKYw7okHwBh63Cx3mhUevji5SkEGj3gbwlBURjeEXpOm0qr-ECeKdmagbi_ipiiQB8m8FNwAbx9Z-Sl3nbJ-kS3QtPZrFHqxf91sgFY16H6sn1ruhna-ZygG5cYKf4JWbEcmLrSmdQ_xIBODjWDcatvNKGrv7Cx_Ng"
})",
                                                                                      boost::beast::http::verb::post);
    nlohmann::json authResJson = nlohmann::json::parse(authRes.body());
    ws->set_option(boost::beast::websocket::stream_base::decorator(
        [port, authResJson](boost::beast::websocket::request_type& req) {
            req.set(boost::beast::http::field::host, "127.0.0.1:" + std::to_string(port));
            req.set(boost::beast::http::field::authorization, "Bearer " + authResJson["pragmaTokens"]["pragmaGameToken"].get<std::string>());
            req.set(boost::beast::http::field::sec_websocket_extensions, "permessage-deflate; client_max_window_bits");
        }));
    const auto results = resolver.resolve("127.0.0.1", std::to_string(port));
    boost::asio::connect(ws->next_layer(), results.begin(), results.end());
    ws->handshake("127.0.0.1:" + std::to_string(port), "/");
}

TestWebsocketClient::~TestWebsocketClient() {
    workGuard.reset(); // Allow ioCtx.run() to exit
    ioCtx.stop();      // Force stop
    if (workerThread.joinable()) {
        workerThread.join();
    }
}

std::shared_ptr<boost::beast::websocket::stream<boost::asio::ip::tcp::socket>> TestWebsocketClient::GetRawSocket() {
    return ws;
}

boost::beast::flat_buffer TestWebsocketClient::SendPacket(const nlohmann::json& packet, SpectreRpcType rpcType) {
    auto promise = std::make_shared<std::promise<boost::beast::flat_buffer>>();
    auto future = promise->get_future();
    auto buffer = std::make_shared<boost::beast::flat_buffer>();
    auto fulfilled = std::make_shared<std::atomic<bool>>(false);

    // Increment request ID safely
    int reqId = nextRequestId.fetch_add(1);

    boost::asio::post(ioCtx, [this, packet, rpcType, promise, buffer, fulfilled, reqId]() {
        // Construct JSON using the captured reqId
        std::string final = "{\"requestId\":" + std::to_string(reqId) +
                            R"(,"type":")" + rpcType.GetName() +
                            R"(","payload":)" + packet.dump() + "}";

        // Write then Read logic remains the same...
        ws->async_write(boost::asio::buffer(final),
                        [this, promise, buffer, fulfilled](boost::system::error_code ec, std::size_t) {
                            if (ec) {
                                if (!fulfilled->exchange(true)) promise->set_value({});
                                return;
                            }

                            auto timer = std::make_shared<boost::asio::steady_timer>(ws->get_executor(), std::chrono::seconds(3));

                            // Start the read
                            ws->async_read(*buffer, [promise, buffer, timer, fulfilled](boost::system::error_code ec, std::size_t) {
                                timer->cancel(); // Triggers the timer's handler with boost::asio::error::operation_aborted
                                if (!fulfilled->exchange(true)) {
                                    if (ec)
                                        promise->set_value({});
                                    else
                                        promise->set_value(std::move(*buffer));
                                }
                            });

                            // Timer handler
                            timer->async_wait([this, fulfilled, promise](boost::system::error_code ec) {
                                // ec == operation_aborted means the read finished first
                                if (!ec) {
                                    if (!fulfilled->exchange(true)) {
                                        boost::system::error_code ignored;
                                        // Forcing the socket closed is the only way to break an active async_read
                                        ws->next_layer().close(ignored);
                                        promise->set_value({});
                                    }
                                }
                            });
                        });
    });

    if (future.wait_for(std::chrono::seconds(5)) == std::future_status::ready) {
        return future.get();
    }
    return {};
}
