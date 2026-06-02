using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Packets;

public record class SpectreWebsocketRequest
{
    public required SpectreRpcType RpcType { get; init; }
    public required JsonElement RequestPayload { get; init; }
    public required Int32 RequestId { get; init; }

    [SetsRequiredMembers]
    public SpectreWebsocketRequest(JsonDocument fullRequest)
    {
        string? rpcType = fullRequest.RootElement.GetProperty("type").GetString() ?? throw new InvalidDataException("rpcType not found in websocket message");
        RpcType = new(rpcType);
        RequestPayload = fullRequest.RootElement.GetProperty("payload");
        RequestId = fullRequest.RootElement.GetProperty("requestId").GetInt32();
    }

    [SetsRequiredMembers]
    public SpectreWebsocketRequest(SpectreRpcType rpcType, JsonElement requestPayload, int requestId)
    {
        RpcType = rpcType;
        RequestPayload = requestPayload;
        RequestId = requestId;
    }
}