#include <SteamAuthTicket.h>
#include <cstdint>

constexpr size_t kGcTokenLengthFieldSize = 4;
constexpr size_t kGcTokenSize = 20;
constexpr size_t kSteamIdOffset = 12;
constexpr uint32_t kExpectedGcTokenSize = 20;

static int HexNibble(char c) {
    if (c >= '0' && c <= '9') return c - '0';
    if (c >= 'A' && c <= 'F') return c - 'A' + 10;
    if (c >= 'a' && c <= 'f') return c - 'a' + 10;
    return -1;
}

static uint32_t ReadU32Le(const uint8_t* buf) {
    return static_cast<uint32_t>(buf[0]) |
           (static_cast<uint32_t>(buf[1]) << 8) |
           (static_cast<uint32_t>(buf[2]) << 16) |
           (static_cast<uint32_t>(buf[3]) << 24);
}

static uint64_t ReadU64Le(const uint8_t* buf) {
    return static_cast<uint64_t>(buf[0]) |
           (static_cast<uint64_t>(buf[1]) << 8) |
           (static_cast<uint64_t>(buf[2]) << 16) |
           (static_cast<uint64_t>(buf[3]) << 24) |
           (static_cast<uint64_t>(buf[4]) << 32) |
           (static_cast<uint64_t>(buf[5]) << 40) |
           (static_cast<uint64_t>(buf[6]) << 48) |
           (static_cast<uint64_t>(buf[7]) << 56);
}

std::string SteamAuthTicket::ExtractSteamId64(const std::string& providerToken) {
    constexpr size_t headerBytes = kGcTokenLengthFieldSize + kGcTokenSize;
    constexpr size_t requiredHexChars = headerBytes * 2;
    if (providerToken.size() < requiredHexChars) return {};

    uint8_t bytes[headerBytes];
    for (size_t i = 0; i < headerBytes; i++) {
        const int hi = HexNibble(providerToken[i * 2]);
        const int lo = HexNibble(providerToken[(i * 2) + 1]);
        if (hi < 0 || lo < 0) return {};
        bytes[i] = static_cast<uint8_t>((hi << 4) | lo);
    }

    if (ReadU32Le(bytes) != kExpectedGcTokenSize) return {};

    const uint64_t steamId = ReadU64Le(bytes + kSteamIdOffset);
    if (((steamId >> 56) & 0xFFU) != 1) return {};
    if (((steamId >> 52) & 0xFU) != 1) return {};

    return std::to_string(steamId);
}