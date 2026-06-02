using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace Packets;

public record class HTTPRoutingIdentifier
{
    [SetsRequiredMembers]
    public HTTPRoutingIdentifier(HttpMethod method, string route)
    {
        Method = method;
        Route = route;
    }

    public required HttpMethod Method { get; set; }
    public required string Route { get; set; }
}

public abstract class HTTPPacketHandler
{
    private static readonly Dictionary<HTTPRoutingIdentifier, HTTPPacketHandler> _handlers = new();

    [SetsRequiredMembers]
    protected HTTPPacketHandler(HttpMethod method, string route)
    {
        Method = method;
        Route = route;
        _handlers[new HTTPRoutingIdentifier(method, route)] = this;
    }

    public required HttpMethod Method { get; init; }
    public required string Route { get; init; }
    public abstract Task<IResult> HandleAsync(HttpContext Request);

    public static HTTPPacketHandler? GetHandlerForRoute(HttpMethod method, string route)
    {
        if(_handlers.TryGetValue(new HTTPRoutingIdentifier(method, route), out HTTPPacketHandler? handler))
        {
            return handler;
        }
        return null;
    }

    public static HTTPPacketHandler? GetHandlerForRoute(HTTPRoutingIdentifier id)
    {
        if(_handlers.TryGetValue(id, out HTTPPacketHandler? handler))
        {
            return handler;
        }
        return null;
    }
}