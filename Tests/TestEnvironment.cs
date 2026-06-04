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
        .AddEnvironmentVariables()
        .AddJsonFile("resources/env.json", optional: true, reloadOnChange: true)
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
        // base directory is Proj/bin/Debug/dotnetver/Executable
        string slnDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        string dockerDir = Path.Combine(slnDir, "Docker");
        string backendDockerFile = Path.Combine(dockerDir, "Backend.dockerfile");
        string pgDockerFile = Path.Combine(dockerDir, "Postgres.dockerfile");
        BuildImage(dockerDir, "pragmabackend", backendDockerFile, "..");
        BuildImage(dockerDir, "pragmabackend-pgdb", pgDockerFile, ".");
        RunDockerCommand("compose down -v", dockerDir);
    }

    [GlobalTestInitialize]
    public static void InitializeEnvironment(TestContext _)
    {
        string slnDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        string dockerDir = Path.Combine(slnDir, "Docker");
        RunDockerCommand("compose up -d", dockerDir);
        Thread.Sleep(TimeSpan.FromSeconds(15)); // Todo replace with a healthcheck on the backend instance
        PostgresDatabase.InstantiateDatabase(config);
    }

    [GlobalTestCleanup]
    public static void CleanupEnvironment(TestContext _)
    {
        PostgresDatabase.Get().ShutdownConnection();
        RunDockerCommand("compose down -v");
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