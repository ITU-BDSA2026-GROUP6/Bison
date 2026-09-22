namespace Bison.Tests;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleDB;

public class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>
{
    //Inistialize HttpClient to be used for testing
    private readonly HttpClient _client;
    public UnitTest1(WebApplicationFactory<Program> fixture)
    {
        _client = fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true, HandleCookies = true });
        Bison.CLI.Program.client = _client;
    }

    [Fact]
    //test to check if comment function returns false if observation does not exist
    public async Task Comment_ReturnsFalse_WhenObservationDoesNotExist()
    {
        //Arrange
        await _client.PostAsJsonAsync("/observation", new Observation(1, "seed", "seeb obs", "Test Location", 1700000000));

        //Act
        var result = Bison.CLI.Program.Comment("Test comment.", 999);

        //Assert
        Assert.False(result);
    }

    
    [Fact]
    //test to check if comment function returns true if observation exists
    public async Task Comment_ReturnsTrue_WhenObservationExists()
    {
        //Arrange
        await _client.PostAsJsonAsync("/observation", new Observation(1, "seed", "seeb obs", "Test Location", 1700000000));

        //Act
        var result = Bison.CLI.Program.Comment("Test comment.", 1);

        //Assert
        Assert.True(result);
    }
    
    

    [Fact]
    //test to check if the timestamp fucktions returns in the correct format
    public void ConvertTimestampToDateTimeString_ReturnsCorrectFormat()
    {
        // Arrange
        long timestamp = 1700000000; // Example timestamp
        string expectedDateTimeString = "2023-11-14 22:13:20"; // Expected output for the given timestamp

        // Act
        string result = UserInterface.ConvertTimestampToDateTimeString(timestamp);

        // Assert
        Assert.Equal(expectedDateTimeString, result);
    }

    [Fact]
    //test to check if the display observations function prints the observation with location correctly
    public void DisplayObservations_PrintsObservationWithLocation()
    {
        // Arrange
        var observation = new Observation(
        1,
        "vitusjh",
        "fugl spottet",
        "Sydhavnen",
        1700000000);

        using var writer = new StringWriter();
        var originalOutput = Console.Out;
        Console.SetOut(writer);

        try
        {
            // Act
            UserInterface.DisplayObservations(new[] { observation });

            // Assert
            string output = writer.ToString();

            Assert.Contains("ID: 1", output);
            Assert.Contains("Author: vitusjh", output);
            Assert.Contains("Observation: fugl spottet", output);
            Assert.Contains("Location: Sydhavnen", output);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }  

    [Fact]
    //test to check if the display observations function prints the observation without location correctly
    public async Task Propose_ReturnsFalse_WhenObservationDoesNotExist()
    {
    await _client.PostAsJsonAsync("/observation", new Observation(1, "seed", "seed obs", "Test Location", 1700000000));
    var result = Bison.CLI.Program.Propose("MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea", 999);
    Assert.False(result);
    }

    [Fact]
    //test to check if the propose function returns false when the taxon does not exist
    public async Task Propose_ReturnsFalse_WhenTaxonDoesNotExist()
    {
    await _client.PostAsJsonAsync("/observation", new Observation(1, "seed", "seed obs", "Test Location", 1700000000));
    var result = Bison.CLI.Program.Propose("not-a-real-taxon-id", 1);
    Assert.False(result);
    }

    [Fact]
    //test to check if the propose function returns true when both the observation and taxon exist
    public async Task Propose_ReturnsTrue_WhenObservationAndTaxonExist()
    {
    await _client.PostAsJsonAsync("/observation", new Observation(1, "seed", "seed obs", "Test Location", 1700000000));
    var result = Bison.CLI.Program.Propose("MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea", 1);
    Assert.True(result);
    }
}