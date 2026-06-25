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
        BuildImage(slnDir, "pragmabackend", backendDockerFile, ".");
        BuildImage(slnDir, "pragmabackend-pgdb", pgDockerFile, ".");
        RunDockerCommand("compose down -v", slnDir);
    }

    [GlobalTestInitialize]
    public static void InitializeEnvironment(TestContext _)
    {
        string slnDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        RunDockerCommand("compose up -d", slnDir);
        while (true)
        {
            var output = RunDockerCommand("inspect --format='{{.State.Health.Status}}' pragmabackend");
            if (output.Output.Contains("healthy"))
            {
                break;
            }
        }
        PostgresDatabase.InstantiateDatabase(config);
    }

    [GlobalTestCleanup]
    public static void CleanupEnvironment(TestContext _)
    {
        PostgresDatabase.Get().ShutdownConnection();
        string slnDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        RunDockerCommand("compose down -v", slnDir);
    }

    [AssemblyCleanup]
    public static void CleanupAssembly(TestContext _)
    {
        if (PostgresDatabase.IsInstantiated())
        {
            PostgresDatabase.Get().ShutdownConnection();
        }
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