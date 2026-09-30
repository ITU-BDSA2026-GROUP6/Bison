using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bison.Razor.Tests;

public class PublicPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PublicPageTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("BISONDBPATH", TestHelpers.GetSeededDbPath());

        _client = factory.CreateClient();
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