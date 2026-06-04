namespace Packets;

public interface IHTTPPacketHandlerSingleton
{
    public abstract static HttpMethod GetMethod();
    public abstract static string GetRoute();
}