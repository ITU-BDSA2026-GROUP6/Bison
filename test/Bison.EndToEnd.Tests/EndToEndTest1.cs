namespace Bison.EndToEnd.Tests;

using System.Diagnostics;
using System.Net.Http.Json;
using SimpleDB;
using Xunit.Sdk;

public class EndToEndTest1
{

    // Path to the Bison.CLI.csproj file
    private static readonly string PathToCliCsproj = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "../../../../../src/BisonCLI/Bison.CLI.csproj"));

    private static readonly string PathToWebServiceCsproj = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../../src/CSVDatabase.WebService/CSVDatabase.WebService.csproj"));

    // This method runs the Bison.CLI with the specified arguments and captures its output and exit code.
    private static (string stdout, int exitcode) RunProcess(string arguments, string workingDirectory)
    {
        // Set up the process start info to run the Bison.CLI project
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{PathToCliCsproj}\" -- {arguments}",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi)!; //the "!" tells the compiler that we are sure it will not be null
        string stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (stdout, process.ExitCode);

    }


    [Fact]
    public async Task calling_bison_read_shows_observation_stored_in_web_service()
    {
        //Arrange: start a real web service
        using var server = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{PathToWebServiceCsproj}\" --urls http://localhost:5204",
            UseShellExecute = false, 
        })!;

        try
        {
            using var client = new HttpClient {BaseAddress = new Uri("http://localhost:5204")};
            await WaitForServer(client);

            string text = $"saw a penguing {Guid.NewGuid()}";
            var response = await client.PostAsJsonAsync("/observation", new Observation(1, "August", text, "Test Location", 1700000000));
            response.EnsureSuccessStatusCode();

            // Act
            var (stdout, exitcode) = RunProcess("read", Path.GetTempPath());

            // Assert
            Assert.Equal(0, exitcode);
            Assert.Contains("Author: August", stdout);
            Assert.Contains($"Observation: {text}", stdout);
        }
        finally
        {
            // Stop webservice
            server.Kill(entireProcessTree: true);
        }
    }

        // Polls the web service until it answers (or gives up after 30 seconds)
    private static async Task WaitForServer(HttpClient client)
    {
        for (int i = 0; i < 60; i++)
        {
            try
            {
                var response = await client.GetAsync("/observations");
                if (response.IsSuccessStatusCode) return;
            }
            catch (HttpRequestException)
            {
                // not started yet
            }
            await Task.Delay(500);
        }
        throw new TimeoutException("Web service did not start");
    }
}
