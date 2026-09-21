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

    [Fact]
    public async Task PostRequestToProposalResponseIs200()
    {
    var proposal = new Proposal(1, "August", "MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea", 1700000000);
    var response = await _client.PostAsJsonAsync("/proposal", proposal);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Fact]
    public async Task GetRequestToProposalsResponseIs200AndListOfProposals()
    {
    await _client.PostAsJsonAsync("/observation", new Observation(1, "August", "Saw a heron", "Copenhagen", 1700000000));
    await _client.PostAsJsonAsync("/proposal", new Proposal(1, "August", "MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea", 1700000000));

    var response = await _client.GetAsync("/proposals?id=1");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var proposals = await response.Content.ReadFromJsonAsync<List<Proposal>>();
    Assert.NotNull(proposals);
    Assert.Contains(proposals, p => p.ObsID == 1);
    }
}