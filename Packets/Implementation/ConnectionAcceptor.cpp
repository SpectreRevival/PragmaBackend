#include "PlayerConnectionThread.h"

#include <ConnectionAcceptor.h>

ConnectionAcceptor::ConnectionAcceptor(boost::asio::io_context& ioc, unsigned short port)
    : ioCtx(ioc), acceptor(ioCtx, boost::asio::ip::tcp::endpoint(boost::asio::ip::make_address("0.0.0.0"), port)) {
    BeginAccepting();
}

void ConnectionAcceptor::BeginAccepting() {
    acceptor.async_accept([this](boost::system::error_code ec, boost::asio::ip::tcp::socket socket) {
        if (!ec) {
            new PlayerConnectionThread(std::move(socket), ioCtx);
        }
        BeginAccepting();
    });
}