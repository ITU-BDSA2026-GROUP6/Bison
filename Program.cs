using SimpleDB;

public record Cheep(string Author, string Observation, long Timestamp);

class Program
{
    static readonly IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(Path.Combine("CSVfiles", "bison_observe_cli_db.csv"));
    
    static void Main(string [] args)
    {
        try
        {
            if (args[0] == "observe")
            {
                Observe(string.Join(" ", args[1..]));
            }
            else
            {
                Console.WriteLine("Invalid command. Use 'observe' followed by observation text.");
            }
            
            foreach (Cheep cheep in database.Read())
            {
                Console.WriteLine($"Author: {cheep.Author}, Observation: {cheep.Observation}, Timestamp: {cheep.Timestamp}");
            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't read file: {e.Message}");
        }
    }

    static void Observe(string message)
    {
        Cheep cheep = new Cheep(Environment.UserName, message, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        try
        {
            database.Store(cheep);
            Console.WriteLine("Observation saved successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't write to file: {e.Message}");
        }
    }
}