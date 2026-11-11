using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bison.Razor.Tests;

public sealed class PostObservationUnitTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PostObservationUnitTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task PostingObservationCreatesCheep()
    {
        const string observation = "A test observation";
        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("message", observation)
        });

        using var response = await _client.PostAsync("/PostObservation", form);

        Assert.True(response.StatusCode == HttpStatusCode.Redirect ||
                    response.StatusCode == HttpStatusCode.SeeOther,
            $"Expected a redirect after posting an observation, got {(int)response.StatusCode}.");
    }
}
