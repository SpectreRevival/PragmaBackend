using System.Text.Json.Nodes;

namespace Tests;

public record class WebsocketTestData
{
    public required string rpcType { get; set; }
    public bool? ignoreReplace { get; set; }
    public required double testAge { get; set; }
    public required JsonObject requestBody { get; set; }
    public required JsonObject responsePayload { get; set; }
}