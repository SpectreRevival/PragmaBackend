using System.Text.Json.Nodes;

namespace Tests;

public record class SpectreWebsocketResponseInner
{
    public required int requestId { get; set; }
    public required string type { get; set; }
    public required JsonObject payload { get; set; }
}