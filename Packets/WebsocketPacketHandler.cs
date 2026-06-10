using Google.Protobuf;
using System.Diagnostics.CodeAnalysis;

namespace Packets;

public abstract class WebsocketPacketProcessor
{
    private static readonly Dictionary<SpectreRpcType, WebsocketPacketProcessor> processors = new();
    public required SpectreRpcType RpcType { get; init; }

    [SetsRequiredMembers]
    protected WebsocketPacketProcessor(SpectreRpcType rpcType)
    {
        RpcType = rpcType;
        processors.Add(rpcType, this);
    }
    public abstract Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler);

    public static WebsocketPacketProcessor? GetProcessorForRequestType(SpectreRpcType rpcType)
    {
        if(processors.TryGetValue(rpcType, out WebsocketPacketProcessor? processor))
        {
            return processor;
        }
        return null;
    }

    private static readonly List<WebsocketPacketProcessor> singletons = new();

    public static void InstantiateSingletons()
    {
        Type singletonInterface = typeof(IWebsocketPacketProcessorSingleton);
        List<Type> singletonClasses = singletonInterface.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
            .Any(i => i == singletonInterface)
            ).ToList();
        foreach (Type singletonClass in singletonClasses)
        {
            var rpcTypeGetter = singletonClass.GetMethod("GetRpcType", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var rpcType = rpcTypeGetter.Invoke(null, new object[] { });
            var ctor = singletonClass.GetConstructors().First();
            singletons.Add((WebsocketPacketProcessor)ctor.Invoke(new object[]
            {
                rpcType
            }));
        }
    }
}