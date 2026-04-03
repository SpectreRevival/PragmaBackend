#include <StaticResponseProcessorHTTP.h>

StaticResponseProcessorHTTP::StaticResponseProcessorHTTP(HTTPRequestIdentifier id, const nlohmann::json& res)
    : HTTPPacketProcessor(id), staticRes(res.dump()){

}

std::optional<restinio::response_builder_t<restinio::restinio_controlled_output_t>> StaticResponseProcessorHTTP::Process(restinio::request_handle_t req, restinio::router::route_params_t params) {
    return req->create_response().set_body(staticRes);
}