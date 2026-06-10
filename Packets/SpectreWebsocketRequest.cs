using Google.Protobuf;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Packets;

public record class SpectreWebsocketRequest
{
    public required SpectreRpcType RpcType { get; init; }
    public required JsonObject RequestPayload { get; init; }
    public required Int32 RequestId { get; init; }

    [SetsRequiredMembers]
    public SpectreWebsocketRequest(JsonDocument fullRequest)
    {
        string? rpcType = fullRequest.RootElement.GetProperty("type").GetString() ?? throw new InvalidDataException("rpcType not found in websocket message");
        RpcType = new(rpcType);
        RequestPayload = JsonNode.Parse(fullRequest.RootElement.GetProperty("payload").GetRawText())!.AsObject();
        RequestId = fullRequest.RootElement.GetProperty("requestId").GetInt32();
    }

    [SetsRequiredMembers]
    public SpectreWebsocketRequest(SpectreRpcType rpcType, JsonObject requestPayload, int requestId)
    {
        RpcType = rpcType;
        RequestPayload = requestPayload;
        RequestId = requestId;
    }

    public T GetPayloadAsMessage<T>() where T : class, IMessage<T>, new()
    {
        var parser = new JsonParser(JsonParser.Settings.Default);
        return parser.Parse<T>(RequestPayload.ToJsonString());
    }
}