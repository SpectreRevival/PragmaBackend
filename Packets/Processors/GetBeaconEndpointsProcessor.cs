using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

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
        if(type == "hathora")
        {
            return SpectreWebsocketMessage.From(hathoraResponse);
        } else
        {
            return SpectreWebsocketMessage.From(hathoraUdpResponse);
        }
    }
}