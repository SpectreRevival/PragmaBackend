#pragma once
#include "boost/asio/io_context.hpp"
#include "boost/asio/ip/tcp.hpp"

class ConnectionAcceptor {
private:
    boost::asio::io_context& ioCtx;
    boost::asio::ip::tcp::acceptor acceptor;
    void BeginAccepting();
public:
    explicit ConnectionAcceptor(boost::asio::io_context& ioc, unsigned short port);
};