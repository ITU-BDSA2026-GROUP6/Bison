namespace Bison.Razor.Tests;

public static class TestHelpers
{
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Bison.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate repo root (Bison.sln not found).");
    }

    public static string GetSeededDbPath()
    {
        return Path.Combine(FindRepoRoot(), "src", "Bison.Razor", "bison.db");
    }
}