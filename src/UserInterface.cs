using System.Globalization;
public static class UserInterface
{
    public static void DisplayObservations(IEnumerable<Observation> observations)
    {
        foreach (Observation observation in observations)
        {
            System.Console.WriteLine(
                $"ID: {observation.ObsID}, " +
                $"Author: {observation.Author}, " +
               $"Observation: {observation.Text}, " +
                $"Timestamp: {UserInterface.ConvertTimestampToDateTimeString(observation.Timestamp)}"
            );
        }
    }

    public static void DisplayDiscussion(Observation observation, IEnumerable<Comment> comments)
    {
        System.Console.WriteLine(
            $"ID: {observation.ObsID}, " +
            $"Author: {observation.Author}, " +
            $"Observation: {observation.Text}, " +
            $"Location: {observation.Location}, " +
            $"Timestamp: {observation.Timestamp}"
        );

        foreach (Comment c in comments)
        {
            System.Console.WriteLine(
                $"Author: {c.Author}, " +
                $"Comment: {c.Text}, " +
                $"Timestamp: {c.Timestamp}"
            );
            
        }
    }

    public static void DisplayInvalidCommandMessage()
    {
        Console.WriteLine("Invalid command. Use 'observe' followed by observation text.");
    }

    public static void DisplaySuccess()
    {
        Console.WriteLine("Observation saved successfully.");
    }

    public static void DisplayWriteError(Exception e)
    {
        Console.WriteLine($"Couldn't write to file: {e.Message}");
    }

    public static void DisplayReadError(Exception e)
    {
        Console.WriteLine($"Couldn't read file: {e.Message}");
    }

    public static string ConvertTimestampToDateTimeString(long timestamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        return dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }
}