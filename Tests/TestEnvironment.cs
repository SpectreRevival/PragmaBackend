using Docker.DotNet;
using Microsoft.Extensions.Configuration;
using Persistence;
using System.Diagnostics;
using System.Text;

namespace Tests;

[TestClass]
public class TestEnvironment
{
    private static readonly IConfiguration config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(Path.Combine("resources", "test.env.json"), optional: false, reloadOnChange: true)
        .Build();
    public static bool BuildImage(string workingDirectory, string imageName, string dockerfilePath, string contextDir)
    {
        string arguments = $"build -t {imageName}:latest -f \"{dockerfilePath}\" \"{contextDir}\"";

        var result = RunDockerCommand(arguments, workingDirectory);
        return result.ExitCode == 0;
    }

    [AssemblyInitialize]
    public static void BuildImages(TestContext _)
    {
        // base directory is .../PragmaBackend/Proj/bin/Debug/dotnetver/, just walk all the way back up to PragmaBackend and get the path of that dir
        string slnDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        string backendDockerFile = Path.Combine(slnDir, "Backend.dockerfile");
        string pgDockerFile = Path.Combine(slnDir, "Postgres.dockerfile");
        Console.WriteLine("Building pragmabackend docker image");
        bool backendBuilt = BuildImage(slnDir, "pragmabackend", backendDockerFile, ".");
        if (!backendBuilt)
        {
            throw new Exception("Failed to build pragmabackend image");
        }
        Console.WriteLine("Building pragmabackend-pgdb docker image");
        bool dbBuilt = BuildImage(slnDir, "pragmabackend-pgdb", pgDockerFile, ".");
        if (!dbBuilt)
        {
            throw new Exception("Failed to built pragmabackend-pgdb image");
        }
        Console.WriteLine("Finished building images, tearing down old compose setup");
        RunDockerCommand("compose down -v", slnDir);
    }

    [GlobalTestInitialize]
    public static void InitializeEnvironment(TestContext _)
    {
        string slnDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        Console.WriteLine("Starting up backend compose...");
        RunDockerCommand("compose up -d", slnDir);
        while (true)
        {
            var output = RunDockerCommand("inspect --format='{{.State.Health.Status}}' pragmabackend");
            if (output.Output.Contains("healthy"))
            {
                break;
            }
            Console.WriteLine("Backend not healthy yet, waiting...");
        }
        Console.WriteLine("got healthy status, initializing connection to test db");
        PostgresDatabase.InstantiateDatabase(config);
        Console.WriteLine("db connection established, running test");
    }

    [GlobalTestCleanup]
    public static void CleanupEnvironment(TestContext _)
    {
        Console.WriteLine("Shutting down connection to db");
        PostgresDatabase.Get().ShutdownConnection();
        string slnDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        Console.WriteLine("Fetching logs...");
        var logcmdOutput = RunDockerCommand("logs pragmabackend");
        var dblogcmdOutput = RunDockerCommand("logs pragmabackend-pgdb");
        Console.WriteLine("Backend log:");
        Console.WriteLine(logcmdOutput.Output);
        Console.WriteLine("Database log:");
        Console.WriteLine(dblogcmdOutput.Output);
        Console.WriteLine("Tearing down compose...");
        RunDockerCommand("compose down -v", slnDir);
    }

    [AssemblyCleanup]
    public static void CleanupAssembly(TestContext _)
    {
        if (PostgresDatabase.IsInstantiated())
        {
            Console.WriteLine("Shutting down connection to db");
            PostgresDatabase.Get().ShutdownConnection();
        }
        Console.WriteLine("Tearing down compose...");
        string slnDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        RunDockerCommand("compose down -v", slnDir);
    }

    private static (int ExitCode, string Output, string Error) RunDockerCommand(string arguments, string? workingDir = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrEmpty(workingDir))
        {
            psi.WorkingDirectory = workingDir;
        }

        using var process = new Process { StartInfo = psi };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (s, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();

        return (process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
    }
}