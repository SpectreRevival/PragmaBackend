#pragma once
#include <cstdint>

enum class HTTPRequestType : uint8_t {
    GET,
    POST,
    PUT,
    DEL,
    HEAD
};