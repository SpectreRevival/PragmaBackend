namespace Processors;

public interface IWebsocketPacketProcessorSingleton
{
    static abstract SpectreRpcType GetRpcType();
}