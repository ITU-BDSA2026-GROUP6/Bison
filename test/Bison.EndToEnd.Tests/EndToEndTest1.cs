namespace Bison.EndToEnd.Tests;
using System.Diagnostics;
using SimpleDB;

public class EndToEndTest1
{

    // Path to the Bison.CLI.csproj file
    private static readonly string PathToCliCsproj = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "../../../../../src/Bison.CLI.csproj"));

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
    public void calling_bison_observe_penguin_stores_correct_values()
    {
        // Arrange. Isolated working directory for the test
        string workingDirectory = Path.Combine(Path.GetTempPath(), $"Bison_e2e_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(Path.Combine(workingDirectory, "CSVfiles"));

        var db = CSVDatabase<Observation>.GetInstance(Path.Combine(workingDirectory, "CSVfiles", "bison_observe_cli_db.csv"));
        db.Store(new Observation(1, "August", "saw a penguin", "Test Location", 1700000000));

        // Act. Run the Bison.CLI with the "read" command
        var (stdout, exitcode) = RunProcess("read", workingDirectory);


        //Assert
        Assert.Equal(0, exitcode);
        Assert.Contains("ID: 1", stdout);
        Assert.Contains("Author: August", stdout);
        Assert.Contains("Observation: saw a penguin", stdout);

        Directory.Delete(workingDirectory, recursive: true);
    }
}
