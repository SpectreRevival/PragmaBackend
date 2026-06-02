using System;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Persistence;

public class PostgresDatabase : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private static PostgresDatabase inst = new();

    public PostgresDatabase()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        string host = config["db:host"] ?? "localhost";
        int port = int.Parse(config["db:port"] ?? "5432");
        string user = config["db:username"] ?? throw new InvalidDataException("No username for db provided");
        string password = config["db:password"] ?? throw new InvalidDataException("No password for db provided");
        string dbName = config["db:database"] ?? throw new InvalidDataException("No db name provided");

        var ConnStr = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = user,
            Password = password,
            Pooling = false,
        };

        _dataSource = NpgsqlDataSource.Create(ConnStr.ConnectionString);

        foreach (string fileName in Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), "InitSQL")))
        {
            string sql = File.ReadAllText(fileName);
            _dataSource.CreateCommand(sql).ExecuteNonQuery();
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

    public async ValueTask DisposeAsync()
    {
        if(_dataSource != null)
        {
            await _dataSource.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}