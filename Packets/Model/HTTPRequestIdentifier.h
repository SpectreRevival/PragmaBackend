#pragma once
#include <HTTPRequestType.h>
#include <string>
#include <functional>

class HTTPRequestIdentifier {
private:
    std::string route;
    HTTPRequestType reqType;
public:
    explicit HTTPRequestIdentifier(std::string route, HTTPRequestType reqType);
    HTTPRequestIdentifier(std::string route);
    bool operator==(const HTTPRequestIdentifier& other) const;
    [[nodiscard]] const std::string& GetRoute() const;
    [[nodiscard]] const HTTPRequestType& GetRequestType() const;
};

namespace std {
    template<>
    struct hash<HTTPRequestIdentifier> {
        std::size_t operator()(const HTTPRequestIdentifier& requestIdentifier) const noexcept {
            std::size_t h1 = std::hash<std::string>{}(requestIdentifier.GetRoute());
            std::size_t h2 = std::hash<unsigned char>{}(
                static_cast<unsigned char>(requestIdentifier.GetRequestType())
            );

            // Combine hashes
            h1 ^= h2 + 0x9e3779b97f4a7c15ULL + (h1 << 6) + (h1 >> 2);
            return h1;
        }
    };
}