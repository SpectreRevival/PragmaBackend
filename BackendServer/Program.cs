using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Packets;
using Packets.Processors;
using Persistence;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Net;

string AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
    theme: AnsiConsoleTheme.Code,
    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: Path.Combine(AppDataDirectory, "/logs/pragmabackend-log.txt"),
        rollingInterval: RollingInterval.Day,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

Log.Information("Started logging, initializing postgres next");
IConfiguration config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(Path.Combine("resources", "env.json"), optional: false, reloadOnChange: true)
        .Build();
PostgresDatabase.InstantiateDatabase(config);

var builder = WebApplication.CreateBuilder();
builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration.AddJsonFile(Path.Combine("resources", "env.json"), optional: false, reloadOnChange: true);
builder.WebHost.ConfigureKestrel(opts =>
{
    opts.Configure(builder.Configuration.GetSection("Kestrel"));
});
var backendApp = builder.Build();
backendApp.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

backendApp.Map("{*path}", async (HttpContext context, string? path) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        Log.Information($"Upgrading websocket request from {context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}");
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        var spectreWS = new SpectreWebsocket(context, ws);
        await spectreWS.HandleAsync();
    }
    HttpMethod method = new(context.Request.Method);
    string route = $"/{path}";
    HTTPPacketHandler? handler = HTTPPacketHandler.GetHandlerForRoute(method, route);
    if (handler == null)
    {
        Log.Warning($"No request handler for route {route} and method {method.Method}, dropping backend");
        return Results.NotFound("no request handler for route");
    }
    return await handler.HandleAsync(context);
});

HTTPPacketHandler.InstantiateSingletons();
StaticHTTPResponder.InstantiateResponders();
StaticWebsocketResponder.InstantiateResponders();

try
{
    backendApp.Run();
}
finally
{
    Log.Information("Shutting down...");
    Log.CloseAndFlush();
}