using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Data.Sqlite;

namespace Bison.E2ETests;

// Starts the Bison web app on a free port against a freshly seeded database,
// and shuts it down again when the tests in the class are done.
public class AppFixture : IAsyncLifetime
{
    private Process? _app;
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bison-e2e-{Guid.NewGuid()}.db");

    public string BaseUrl { get; } = $"http://localhost:{GetFreePort()}";

    public async Task InitializeAsync()
    {
        var projectDir = Path.Combine(FindRepoRoot(), "src", "Bison.Razor");
        SeedDatabase(Path.Combine(projectDir, "data"));

        var startInfo = new ProcessStartInfo("dotnet", $"run --no-launch-profile --urls {BaseUrl}")
        {
            WorkingDirectory = projectDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.Environment["BISONDBPATH"] = _dbPath;
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        _app = Process.Start(startInfo)!;
        // Drain the output so the app never blocks on a full buffer.
        _app.OutputDataReceived += (_, _) => { };
        _app.ErrorDataReceived += (_, _) => { };
        _app.BeginOutputReadLine();
        _app.BeginErrorReadLine();

        await WaitUntilRespondingAsync();
    }

    public Task DisposeAsync()
    {
        if (_app is { HasExited: false })
        {
            _app.Kill(entireProcessTree: true);
            _app.WaitForExit();
        }
        _app?.Dispose();

        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
        return Task.CompletedTask;
    }

    private void SeedDatabase(string dataDir)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        foreach (var file in new[] { "schema.sql", "dump.sql" })
        {
            using var command = connection.CreateCommand();
            command.CommandText = File.ReadAllText(Path.Combine(dataDir, file));
            command.ExecuteNonQuery();
        }
    }

    private async Task WaitUntilRespondingAsync()
    {
        using var client = new HttpClient();
        var timeout = Stopwatch.StartNew();
        while (timeout.Elapsed < TimeSpan.FromSeconds(60))
        {
            if (_app!.HasExited)
                throw new InvalidOperationException($"The app exited with code {_app.ExitCode} before it started responding.");
            try
            {
                var response = await client.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode) return;
            }
            catch (HttpRequestException)
            {
                // Not listening yet.
            }
            await Task.Delay(250);
        }
        throw new TimeoutException($"The app did not start responding on {BaseUrl} within 60 seconds.");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Bison.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Could not find Bison.sln above the test directory.");
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}