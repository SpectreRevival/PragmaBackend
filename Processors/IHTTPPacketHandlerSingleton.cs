namespace Processors;

public interface IHTTPPacketHandlerSingleton
{
    static abstract HttpMethod GetMethod();
    static abstract string GetRoute();
}