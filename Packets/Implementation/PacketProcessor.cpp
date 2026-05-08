#include <PacketProcessor.h>
#include <drogon/drogon.h>
#include <utility>
using namespace drogon;

HTTPPacketProcessor::HTTPPacketProcessor(HTTPRequestIdentifier routeId)
    : routeId(std::move(std::move(routeId))) {
    app().registerHandler(this->routeId.GetRoute(),
                          [this](const HttpRequestPtr& req, std::function<void(const HttpResponsePtr&)>&& callback) {
                              // callback MUST fire or drogon abandons the connection silently and the client times out.
                              try {
                                  if (auto res = Process(req); res.has_value()) {
                                      callback(res.value());
                                  } else {
                                      spdlog::warn("Packet of route {} type {} dropped because processor refused to respond", this->routeId.GetRoute(), static_cast<unsigned int>(this->routeId.GetRequestType()));
                                      auto errorRes = HttpResponse::newHttpResponse();
                                      errorRes->setBody("err refused to respond");
                                      errorRes->setStatusCode(k500InternalServerError);
                                      callback(errorRes);
                                  }
                              } catch (const std::exception& e) {
                                  spdlog::error("handler for {} threw: {}", this->routeId.GetRoute(), e.what());
                                  auto errorRes = HttpResponse::newHttpResponse();
                                  errorRes->setBody("err handler exception");
                                  errorRes->setStatusCode(k500InternalServerError);
                                  callback(errorRes);
                              } catch (...) {
                                  spdlog::error("handler for {} threw unknown exception", this->routeId.GetRoute());
                                  auto errorRes = HttpResponse::newHttpResponse();
                                  errorRes->setBody("err handler exception");
                                  errorRes->setStatusCode(k500InternalServerError);
                                  callback(errorRes);
                              }
                          },
                          {this->routeId.GetRequestType()});
}

HTTPPacketProcessor::HTTPPacketProcessor(HTTPRequestIdentifier routeId, uint16_t /*port*/)
    : routeId(std::move(std::move(routeId))) {
}