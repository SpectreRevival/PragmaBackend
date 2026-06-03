namespace Tests;

public record class SpectreWebsocketResponse
{
    public required Int32 sequuenceNumber { get; set; }
    public required SpectreWebsocketResponseInner response { get; set; }
}