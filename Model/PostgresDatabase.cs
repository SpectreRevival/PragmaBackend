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
    private readonly IConfiguration config;

    public PostgresDatabase(IConfiguration config)
    {
        string host = config["DB_HOST"] ?? "localhost";
        int port = int.Parse(config["DB_PORT"] ?? "5432");
        string user = config["DB_USERNAME"] ?? throw new InvalidDataException("No username for db provided");
        string password = config["DB_PASSWORD"] ?? throw new InvalidDataException("No password for db provided");
        string dbName = config["DB_DATABASE"] ?? throw new InvalidDataException("No db name provided");
        this.config = config;

        var ConnStr = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = user,
            Password = password,
            Pooling = false,
            IncludeErrorDetail = config["SENSITIVE_LOGGING"] == "true"
        };

        // We have to do the type initialization before so when NpgsqlDataSourceBuilder is created the types are correct (it fetches them into a local cache on creation)
        using (var conn = new NpgsqlConnection(ConnStr.ConnectionString))
        {
            int connectTimeout = int.Parse(config.GetRequiredSection("DB_CONNECT_TIMEOUT_S").Value);
            if (connectTimeout == 0)
            {
                conn.Open();
            } else
            {
                CancellationTokenSource src = new CancellationTokenSource(TimeSpan.FromSeconds(connectTimeout));
                conn.OpenAsync(src.Token).GetAwaiter().GetResult();
            }
            int currentScriptInitializationLevel = 0;
            string nextDirPath = Path.Combine(AppContext.BaseDirectory, "commands", "init", currentScriptInitializationLevel.ToString());
            while (Directory.Exists(nextDirPath))
            {
                Log.Information($"Beginning script initialization level {currentScriptInitializationLevel}");
                foreach (string filepath in Directory.EnumerateFiles(nextDirPath))
                {
                    Log.Information($"Executing sql script from {filepath}");
                    string sqlScript = File.ReadAllText(filepath);
                    try
                    {
                        new NpgsqlCommand(sqlScript, conn).ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Exception thrown while executing sql script at {filepath}: {ex.Message}");
                    }
                }
                currentScriptInitializationLevel++;
                nextDirPath = Path.Combine(AppContext.BaseDirectory, "commands", "init", currentScriptInitializationLevel.ToString());
            }
        }
        var builder = new NpgsqlDataSourceBuilder(ConnStr.ConnectionString);
        builder.MapComposite<RGBAColor>("rgbacolor");
        builder.MapComposite<CrosshairDot>("crosshairdot");
        builder.MapComposite<PipConfig>("pipconfig");
        builder.MapComposite<WeaponAttachment>("weaponattachment");
        builder.MapComposite<WeaponData>("weapondata");
        builder.MapComposite<ActiveAlterationData>("activealterationdata");
        builder.MapComposite<WeaponAttachment>("weaponattachment");
        builder.MapComposite<WeaponData>("weapondata");
        builder.MapComposite<DisplayName>("displayname");
        builder.MapComposite<OutfitData>("outfitdata");
        builder.MapComposite<AlterationChannel>("alterationchannel");
        builder.MapEnum<PlayerBasicPresence>("playerbasicpresence");
        builder.MapComposite<ResponseCurve>("responsecurve");
        builder.MapComposite<LookSettings>("looksettings");
        builder.MapComposite<LookConfig>("lookconfig");
        builder.MapComposite<PartyMember>("partymember");
        builder.MapEnum<ObjectiveContributionSourceType>("objectivecontributionsourcetype");
        builder.MapComposite<ObjectiveContribution>("objectivecontribution");
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

    public static bool IsInstantiated()
    {
        return inst != null;
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

    public IConfiguration GetConfiguration()
    {
        return config;
    }
}