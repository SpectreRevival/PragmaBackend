#include <PacketProcessor.h>
#include <drogon/drogon.h>
#include <utility>
using namespace drogon;

HTTPPacketProcessor::HTTPPacketProcessor(HTTPRequestIdentifier routeId)
    : routeId(std::move(std::move(routeId))) {
    app().registerHandler("/api/v1/status",
                          [this, routeId](const HttpRequestPtr& req, std::function<void(const HttpResponsePtr&)>&& callback) {
                              if (auto res = Process(req); res.has_value()) {
                                  callback(res.value());
                              } else {
                                  spdlog::warn("Packet of route {} type {} dropped because processor refused to respond", routeId.GetRoute(), static_cast<unsigned int>(routeId.GetRequestType()));
                                  auto errorRes = HttpResponse::newHttpResponse();
                                  errorRes->setStatusCode(k500InternalServerError);
                                  callback(errorRes);
                              }
                          },
                          {routeId.GetRequestType()} // Constraints: only allow GET
    );
}

HTTPPacketProcessor::HTTPPacketProcessor(HTTPRequestIdentifier routeId, uint16_t port)
    : routeId(std::move(std::move(routeId))) {
}