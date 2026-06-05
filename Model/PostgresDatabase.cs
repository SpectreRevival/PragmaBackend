using System;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Serilog;
using Model;

namespace Persistence;

public class PostgresDatabase : IAsyncDisposable, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private static PostgresDatabase? inst;

    public PostgresDatabase(IConfiguration config)
    {
        string host = config["DB_HOST"] ?? "localhost";
        int port = int.Parse(config["DB_PORT"] ?? "5432");
        string user = config["DB_USERNAME"] ?? throw new InvalidDataException("No username for db provided");
        string password = config["DB_PASSWORD"] ?? throw new InvalidDataException("No password for db provided");
        string dbName = config["DB_DATABASE"] ?? throw new InvalidDataException("No db name provided");

        var ConnStr = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = user,
            Password = password,
            Pooling = false,
            IncludeErrorDetail = config["SENSITIVE_LOGGING"] == "true"
        };
        var builder = new NpgsqlDataSourceBuilder(ConnStr.ConnectionString);
        builder.MapComposite<RGBAColor>("rgbacolor");
        builder.MapComposite<CrosshairDot>("crosshairdot");
        builder.MapComposite<PipConfig>("pipconfig");
        builder.MapComposite<WeaponAttachment>("weaponattachment");
        builder.MapComposite<WeaponData>("weapondata");
        builder.MapComposite<ActiveAlterationData>("activealterationdata");
        builder.MapComposite<WeaponAttachment>("weaponattachment");
        builder.MapComposite<WeaponData>("weapondata");
        _dataSource = builder.Build();
        while (true)
        {
            try
            {
                Log.Information("Attempting to connect to database");
                using var connection = _dataSource.OpenConnection();
                break;
            } catch (Exception ex) when (ex is SocketException || ex is PostgresException || ex is NpgsqlException)
            {
                Log.Warning("Database not ready yet... trying again in 5s");
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }

    public NpgsqlDataSource GetRaw()
    {
        return _dataSource;
    }

    public static PostgresDatabase Get()
    {
        return inst;
    }

    public static void InstantiateDatabase(IConfiguration config)
    {
        inst = new(config);
    }

    /** 
     * @brief Loads a SQL command from a path relative to the commands base directory
     */
    public static NpgsqlCommand LoadCommandFromFile(string relPath)
    {
        string fullPath = Path.Combine(Path.Combine(AppContext.BaseDirectory, "commands"), relPath);
        string sqlCommandText = File.ReadAllText(fullPath);
        return Get().GetRaw().CreateCommand(sqlCommandText);
    }

    public async ValueTask DisposeAsync()
    {
        if(_dataSource != null)
        {
            await _dataSource.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        var awaiter = DisposeAsync().GetAwaiter();
        while (!awaiter.IsCompleted)
        {

        }
    }

    public void ShutdownConnection()
    {
        Dispose();
        inst = null;
    }
}