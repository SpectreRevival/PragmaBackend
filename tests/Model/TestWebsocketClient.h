#pragma once
#include "boost/asio/ip/tcp.hpp"
#include "boost/asio/executor_work_guard.hpp" // Added
#include "boost/beast/websocket/stream.hpp"
#include "nlohmann/json_fwd.hpp"
#include <SpectreRpcType.h>
#include <string>
#include <thread>
#include <future>
#include <atomic>

class TestWebsocketClient {
    boost::asio::io_context ioCtx;
    // Keeps ioCtx.run() from returning immediately
    boost::asio::executor_work_guard<boost::asio::io_context::executor_type> workGuard;
    std::thread workerThread;

    std::shared_ptr<boost::beast::websocket::stream<boost::asio::ip::tcp::socket>> ws;
    std::atomic<int> nextRequestId; // Thread-safe increment

public:
    explicit TestWebsocketClient(unsigned short port);
    ~TestWebsocketClient(); // Crucial for cleanup

    std::shared_ptr<boost::beast::websocket::stream<boost::asio::ip::tcp::socket>> GetRawSocket();
    [[nodiscard]] boost::beast::flat_buffer SendPacket(const nlohmann::json& packet, SpectreRpcType rpcType);
};