namespace Bison.Tests;
using SimpleDB;

public class UnitTest1
{
    [Fact]
    public void Comment_ReturnsFalse_WhenObservationDoesNotExist()
    {
        // Arrange
        string temporaryFilePath = Path.Combine(Path.GetTempPath(), "test_bison_observe_cli_db.csv");
        string temporaryCommentFilePath = Path.Combine(Path.GetTempPath(), "test_bison_comment_cli_db.csv");

        Program.database = new CSVDatabase<Observation>.GetInstance(temporaryFilePath);
        Program.commentDatabase = new CSVDatabase<Comment>.GetInstance(temporaryCommentFilePath);

        Program.database.Store(new Observation(1, "seed", "seed obs", "Test Location", 1700000000));

        var nonExistentObsID = 999; // Assuming this ID does not exist in the database
        var commentText = "This is a test comment.";

        // Act
        var result = Program.Comment(commentText, nonExistentObsID);

        // Assert
        Assert.False(result);

        // Cleanup
        File.Delete(temporaryFilePath);
        File.Delete(temporaryCommentFilePath);
    }

    
    [Fact]
    public void Comment_ReturnsTrue_WhenObservationExists()
    {
        string temporaryFilePath = Path.Combine(Path.GetTempPath(), "test_bison_observe_cli_db.csv");
        string temporaryCommentFilePath = Path.Combine(Path.GetTempPath(), "test_bison_comment_cli_db.csv");

        Program.database = new CSVDatabase<Observation>.GetInstance(temporaryFilePath);
        Program.commentDatabase = new CSVDatabase<Comment>.GetInstance(temporaryCommentFilePath);

        Program.database.Store(new Observation(1, "seed", "seed obs", "Test Location", 1700000000));

        var existingObsID = 1; // Assuming this ID exists in the database
        var commentText = "This is a test comment.";

        // Act
        var result = Program.Comment(commentText, existingObsID);

        // Assert
        Assert.True(result);
    }
    
    

    [Fact]
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
}