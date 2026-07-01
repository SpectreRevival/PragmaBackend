using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Processors;

public abstract class WebsocketPacketProcessor
{
    private static readonly Dictionary<SpectreRpcType, WebsocketPacketProcessor> processors = [];
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
        return processors.TryGetValue(rpcType, out WebsocketPacketProcessor? processor) ? processor : null;
    }

    public static readonly List<WebsocketPacketProcessor> singletons = [];

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
            MethodInfo? rpcTypeGetter = singletonClass.GetMethod("GetRpcType", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            object? rpcType = rpcTypeGetter.Invoke(null, new object[] { });
            ConstructorInfo ctor = singletonClass.GetConstructors().First();
            singletons.Add((WebsocketPacketProcessor)ctor.Invoke(new object[]
            {
                rpcType
            }));
        }
    }
}