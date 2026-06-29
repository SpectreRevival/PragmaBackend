namespace Tests;

public record class SpectreWebsocketResponse
{
    public required int sequenceNumber { get; set; }
    public required SpectreWebsocketResponseInner response { get; set; }
}