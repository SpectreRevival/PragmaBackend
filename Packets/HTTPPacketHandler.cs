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

    private static readonly List<HTTPPacketHandler> singletons = new();

    public static void InstantiateSingletons()
    {
        Type singletonInterface = typeof(IHTTPPacketHandlerSingleton);
        List<Type> singletonClasses = singletonInterface.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
            .Any(i => i == singletonInterface)
            ).ToList();
        foreach(Type singletonClass in singletonClasses)
        {
            var httpMethodGetter = singletonClass.GetMethod("GetMethod", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var httpRouteGetter = singletonClass.GetMethod("GetRoute", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var httpMethod = httpMethodGetter.Invoke(null, new object[] { });
            var httpRoute = httpRouteGetter.Invoke(null, new object[] { });
            var ctor = singletonClass.GetConstructors().First();
            singletons.Add((HTTPPacketHandler)ctor.Invoke(new object[]
            {
                httpMethod, httpRoute
            }));
        }
    }
}