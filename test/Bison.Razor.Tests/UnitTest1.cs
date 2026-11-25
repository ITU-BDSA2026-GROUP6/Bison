using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bison.Razor.Tests;

public class PublicPageTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bison-public-{Guid.NewGuid():N}.db");

    public PublicPageTests()
    {
        File.Copy(TestHelpers.GetSeededDbPath(), _dbPath);
        var previousPath = Environment.GetEnvironmentVariable("BISONDBPATH");
        try
        {
            Environment.SetEnvironmentVariable("BISONDBPATH", _dbPath);
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }
        finally
        {
            Environment.SetEnvironmentVariable("BISONDBPATH", previousPath);
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
    }

    [Fact]
    public async Task GetObs_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/obs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetObsForUsername_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/obs/Adrian");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
