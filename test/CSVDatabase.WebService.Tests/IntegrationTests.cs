using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleDB;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public IntegrationTests(WebApplicationFactory<Program> fixture)
    {
        _client = fixture.CreateClient(new WebApplicationFactoryClientOptions {});
    }


    [Fact]
    public async Task GetRequestToObservationsResponseIs200AndListOfObservations()
    {
        var response = await _client.GetAsync("/observations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var observations = await response.Content.ReadFromJsonAsync<List<Observation>>();
        Assert.NotNull(observations);
    }

    [Fact]
    public async Task PostRequestToObservationResponseIs200()
    {
        var observation = new Observation(1, "August", "Saw a heron", "Copenhagen", 1700000000);
        var response = await _client.PostAsJsonAsync("/observation", observation); 
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); 
    }

}