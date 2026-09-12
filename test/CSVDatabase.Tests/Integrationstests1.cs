using SimpleDB;
namespace CSVDatabase.Tests;

public class IntegrationTests
{
    [Fact]
    public void storeThenRetrieve()
    {
        // Arrange
        string temporaryFilePath = Path.Combine(Path.GetTempPath(), $"test_bison_observe_cli_db_{Guid.NewGuid()}.csv");
        var database = new CSVDatabase<Observation>(temporaryFilePath);

        Observation observationToStore = new Observation(1, "test_author", "test_observation", 1700000000);

        // Act
        database.Store(observationToStore);
        var retrievedObservations = database.Read();

        // Assert
        Assert.Single(retrievedObservations);
        Assert.Equal(observationToStore.ObsID, retrievedObservations.First().ObsID);
        Assert.Equal(observationToStore.Author, retrievedObservations.First().Author);
        Assert.Equal(observationToStore.Text, retrievedObservations.First().Text);
        Assert.Equal(observationToStore.Timestamp, retrievedObservations.First().Timestamp);

        // Cleanup
        File.Delete(temporaryFilePath);
    }

    [Fact]
    public void storeMultipleThenRetrieve()
    {
        // Arrange
        string temporaryFilePath = Path.Combine(Path.GetTempPath(), $"test_bison_observe_cli_db_{Guid.NewGuid()}.csv");
        var database = new CSVDatabase<Observation>(temporaryFilePath);

        Observation observation1 = new Observation(1, "author1", "observation1", 1700000000);
        Observation observation2 = new Observation(2, "author2", "observation2", 1700000001);

        // Act
        database.Store(observation1);
        database.Store(observation2);
        var retrievedObservations = database.Read();

        // Assert
        Assert.Equal(2, retrievedObservations.Count());
        Assert.Contains(retrievedObservations, o => o.ObsID == observation1.ObsID);
        Assert.Contains(retrievedObservations, o => o.ObsID == observation2.ObsID);

        // Cleanup
        File.Delete(temporaryFilePath);
    }
}