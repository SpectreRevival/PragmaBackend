namespace Packets;

public interface IWebsocketPacketProcessorSingleton
{
    public abstract static SpectreRpcType GetRpcType();
}