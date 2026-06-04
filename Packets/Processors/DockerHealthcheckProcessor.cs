using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

public class DockerHealthCheckProcessor : HTTPPacketHandler, IHTTPPacketHandlerSingleton
{
    [SetsRequiredMembers]
    public DockerHealthCheckProcessor(HttpMethod method, string route) : base(method, route)
    {
    }

    public static HttpMethod GetMethod()
    {
        return HttpMethod.Get;
    }

    public static string GetRoute()
    {
        return "/instance-healthcheck";
    }

    public override async Task<IResult> HandleAsync(HttpContext Request)
    {
        return Results.Ok("alive :)");
    }
}