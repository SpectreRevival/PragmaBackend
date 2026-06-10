namespace Tests;

public record class SpectreWebsocketResponse
{
    public required Int32 sequenceNumber { get; set; }
    public required SpectreWebsocketResponseInner response { get; set; }
}