using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Processors;

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
    private static readonly Dictionary<HTTPRoutingIdentifier, HTTPPacketHandler> _handlers = [];

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
        return _handlers.TryGetValue(new HTTPRoutingIdentifier(method, route), out HTTPPacketHandler? handler) ? handler : null;
    }

    public static HTTPPacketHandler? GetHandlerForRoute(HTTPRoutingIdentifier id)
    {
        return _handlers.TryGetValue(id, out HTTPPacketHandler? handler) ? handler : null;
    }

    private static readonly List<HTTPPacketHandler> singletons = [];

    public static void InstantiateSingletons()
    {
        Type singletonInterface = typeof(IHTTPPacketHandlerSingleton);
        List<Type> singletonClasses = singletonInterface.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
            .Any(i => i == singletonInterface)
            ).ToList();
        foreach (Type singletonClass in singletonClasses)
        {
            MethodInfo? httpMethodGetter = singletonClass.GetMethod("GetMethod", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            MethodInfo? httpRouteGetter = singletonClass.GetMethod("GetRoute", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            object? httpMethod = httpMethodGetter.Invoke(null, new object[] { });
            object? httpRoute = httpRouteGetter.Invoke(null, new object[] { });
            ConstructorInfo ctor = singletonClass.GetConstructors().First();
            singletons.Add((HTTPPacketHandler)ctor.Invoke(new object[]
            {
                httpMethod, httpRoute
            }));
        }
    }
}