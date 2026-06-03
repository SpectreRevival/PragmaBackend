using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Packets;
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
        .AddEnvironmentVariables()
        .AddJsonFile("resources/env.json", optional: true, reloadOnChange: true)
        .Build();
PostgresDatabase.InstantiateDatabase(config);

int currentScriptInitializationLevel = 0;
string nextDirPath = Path.Combine(Path.Combine(Path.Combine(AppContext.BaseDirectory, "resources"), "InitSQL"), currentScriptInitializationLevel.ToString());
while (Directory.Exists(nextDirPath))
{
    Log.Information($"Beginning script initialization level {currentScriptInitializationLevel}");
    foreach (string filepath in Directory.EnumerateFiles(nextDirPath))
    {
        Log.Information($"Executing sql script from {filepath}");
        string sqlScript = File.ReadAllText(filepath);
        try
        {
            PostgresDatabase.Get().GetRaw().CreateCommand(sqlScript).ExecuteNonQuery();
        } catch (Exception ex)
        {
            Log.Error($"Exception thrown while executing sql script at {filepath}: {ex.Message}");
        }
    }
    currentScriptInitializationLevel++;
    nextDirPath = Path.Combine(Path.Combine(Path.Combine(AppContext.BaseDirectory, "resources"), "InitSQL"), currentScriptInitializationLevel.ToString());
}

var builder = WebApplication.CreateBuilder();
builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("resources/env.json", optional: true, reloadOnChange: true);
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

try
{
    backendApp.Run();
}
finally
{
    Log.Information("Shutting down...");
    Log.CloseAndFlush();
}