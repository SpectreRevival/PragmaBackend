using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class GetBeaconEndpointsProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    private readonly string hathoraResponse;
    private readonly string hathoraUdpResponse;
    [SetsRequiredMembers]
    public GetBeaconEndpointsProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
        hathoraResponse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "resources", "hathora.json"));
        hathoraUdpResponse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "resources", "hathora-udp.json"));
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnBeaconServiceRpc.GetBeaconEndpointsV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        string type = Packet.RequestPayload["type"]!.ToString();
        return type == "hathora" ? SpectreWebsocketMessage.From(hathoraResponse) : SpectreWebsocketMessage.From(hathoraUdpResponse);
    }
}