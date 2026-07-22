using Microsoft.Extensions.Configuration;
using Npgsql;
using Serilog;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Model.Persistence;

public class PostgresDatabase : IAsyncDisposable, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private static PostgresDatabase? inst;
    private static readonly ConcurrentDictionary<string, string> CommandTextCache = new();
    private readonly IConfiguration config;
    private readonly NpgsqlConnection _connection;

    public PostgresDatabase(IConfiguration config)
    {
        string host = config["DB_HOST"] ?? "localhost";
        int port = int.Parse(config["DB_PORT"] ?? "5432");
        string user = config["DB_USERNAME"] ?? throw new InvalidDataException("No username for db provided");
        string password = config["DB_PASSWORD"] ?? throw new InvalidDataException("No password for db provided");
        _ = config["DB_DATABASE"] ?? throw new InvalidDataException("No db name provided");
        this.config = config;
        PreloadCommandTextCache();

        NpgsqlConnectionStringBuilder ConnStr = new()
        {
            Host = host,
            Port = port,
            Username = user,
            Password = password,
            Pooling = true,
            IncludeErrorDetail = config["SENSITIVE_LOGGING"] == "true"
        };
        int connectTimeout = int.Parse(config.GetRequiredSection("DB_CONNECT_TIMEOUT_S").Value);

        // We have to do the type initialization before so when NpgsqlDataSourceBuilder is created the types are correct (it fetches them into a local cache on creation)
        using (NpgsqlConnection conn = new(ConnStr.ConnectionString))
        {
            if (connectTimeout == 0)
            {
                conn.Open();
            }
            else
            {
                CancellationTokenSource src = new(TimeSpan.FromSeconds(connectTimeout));
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
        NpgsqlDataSourceBuilder builder = new(ConnStr.ConnectionString);
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
        builder.MapComposite<ClientMessageSender>("clientmessagesender");
        _dataSource = builder.Build();
        if (connectTimeout == 0)
        {
            _connection = _dataSource.OpenConnection();
        }
        else
        {
            CancellationTokenSource src = new(TimeSpan.FromSeconds(connectTimeout));
            ValueTask<NpgsqlConnection> openConnectionTask = _dataSource.OpenConnectionAsync(src.Token);
            _connection = openConnectionTask.ConfigureAwait(true).GetAwaiter().GetResult();
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

    public static NpgsqlBatch CreateBatch()
    {
        return Get().GetRaw().CreateBatch();
    }

    public static void InstantiateDatabase(IConfiguration config)
    {
        inst = new(config);
    }

    public static bool IsInstantiated()
    {
        return inst != null;
    }

    public static NpgsqlConnection GetActiveConnection()
    {
        return Get()._connection;
    }

    /** 
     * @brief Loads a SQL command from a path relative to the commands base directory
     */

    public static string LoadCommandAsString(string relPath)
    {
        string normalizedPath = NormalizeCommandPath(relPath);
        if (!CommandTextCache.TryGetValue(normalizedPath, out string? sqlCommandText))
        {
            string fullPath = Path.Combine(Path.Combine(AppContext.BaseDirectory, "commands"), normalizedPath);
            sqlCommandText = File.ReadAllText(fullPath);
            CacheCommandText(normalizedPath, sqlCommandText);
        }
        return sqlCommandText;
    }

    public static NpgsqlBinaryImporter LoadBinaryImporter(string relPath)
    {
        return Get().GetRaw().OpenConnection().BeginBinaryImport(LoadCommandAsString(relPath));
    }

    public static NpgsqlBinaryExporter LoadBinaryExporter(string relPath)
    {
        return Get().GetRaw().OpenConnection().BeginBinaryExport(LoadCommandAsString(relPath));
    }

    public static NpgsqlCommand LoadCommandFromFile(string relPath)
    {
        return Get().GetRaw().CreateCommand(LoadCommandAsString(relPath));
    }

    public static NpgsqlBatchCommand LoadBatchCommandFromFile(string relPath)
    {
        return new NpgsqlBatchCommand(LoadCommandAsString(relPath));
    }

    private static void PreloadCommandTextCache()
    {
        string commandsPath = Path.Combine(AppContext.BaseDirectory, "commands");
        if (!Directory.Exists(commandsPath))
        {
            Log.Warning("SQL commands directory not found at {CommandsPath}", commandsPath);
            return;
        }

        int loaded = 0;
        foreach (string fullPath in Directory.EnumerateFiles(commandsPath, "*.sql", SearchOption.AllDirectories))
        {
            string relPath = Path.GetRelativePath(commandsPath, fullPath);
            CacheCommandText(relPath, File.ReadAllText(fullPath));
            loaded++;
        }

        Log.Information("Preloaded {CommandCount} SQL command files", loaded);
    }

    private static void CacheCommandText(string relPath, string sqlCommandText)
    {
        string normalizedPath = NormalizeCommandPath(relPath);
        CommandTextCache[normalizedPath] = sqlCommandText;
        CommandTextCache[normalizedPath.Replace('\\', '/')] = sqlCommandText;
        CommandTextCache[normalizedPath.Replace('/', '\\')] = sqlCommandText;
    }

    private static string NormalizeCommandPath(string relPath)
    {
        return relPath
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
    }

    public static NpgsqlCommand CreateCommand(string cmd)
    {
        return Get().GetRaw().CreateCommand(cmd);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dataSource != null)
        {
            await _dataSource.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        ValueTaskAwaiter awaiter = DisposeAsync().GetAwaiter();
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