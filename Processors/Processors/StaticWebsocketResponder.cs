using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class StaticWebsocketResponder : WebsocketPacketProcessor
{
    private readonly string _responseData;
    private static readonly List<StaticWebsocketResponder> _responders = [];

    [SetsRequiredMembers]
    public StaticWebsocketResponder(SpectreRpcType rpcType, string responseFilePath) : base(rpcType)
    {
        if (!File.Exists(responseFilePath))
        {
            throw new InvalidDataException($"No file at {responseFilePath} to load response data for StaticWebsocketResponder at RpcType {rpcType.GetName()}");
        }
        _responseData = File.ReadAllText(responseFilePath);
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        return SpectreWebsocketMessage.From(_responseData);
    }

    public static void InstantiateResponders()
    {
        string staticWsDir = Path.Combine(AppContext.BaseDirectory, "staticws");
        foreach (string filePath in Directory.EnumerateFiles(staticWsDir, "*", SearchOption.AllDirectories))
        {
            SpectreRpcType rpcType = new(Path.GetFileNameWithoutExtension(filePath));
            if (WebsocketPacketProcessor.GetProcessorForRequestType(rpcType) != null)
            {
                continue;
            }
            _responders.Add(new StaticWebsocketResponder(rpcType, filePath));
        }
    }
}