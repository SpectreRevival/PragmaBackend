using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Packets.Processors;

public class StaticHTTPResponder : HTTPPacketHandler
{
    private readonly string _responseData;
    private static readonly List<StaticHTTPResponder> responders = new();

    [SetsRequiredMembers]
    public StaticHTTPResponder(HttpMethod method, string route, string responseFilePath) : base(method, route)
    {
        if (!File.Exists(responseFilePath))
        {
            throw new InvalidDataException($"No file at {responseFilePath} to load response data for StaticHTTP Responder at route {route} method {method}");
        }
        string resDataUnformatted = File.ReadAllText(responseFilePath);
        _responseData = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(resDataUnformatted));
    }

    public override async Task<IResult> HandleAsync(HttpContext Request)
    {
        return Results.Content(_responseData, "application/json");
    }

    public static void InstantiateResponders()
    {
        string staticHttpDir = Path.Combine(AppContext.BaseDirectory, "httpstatic");
        foreach (string responsePath in Directory.EnumerateFiles(staticHttpDir, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(staticHttpDir, responsePath);
            relative = relative.Replace(Path.DirectorySeparatorChar, '/');
            relative = "/" + Path.ChangeExtension(relative, null);
            responders.Add(new StaticHTTPResponder(HttpMethod.Get, relative, responsePath));
        }
    }
}