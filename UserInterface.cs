public static class UserInterface
{
    public static void DisplayObservations(IEnumerable<Cheep> observations)
    {
        foreach (Cheep cheep in observations)
        {
            Console.WriteLine($"Author: {cheep.Author}, Observation: {cheep.Observation}, Timestamp: {cheep.Timestamp}");
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
}