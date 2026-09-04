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
                UserInterface.DisplayInvalidCommandMessage();
            }
            
            foreach (Cheep cheep in database.Read())
            {
                UserInterface.DisplayObservations(new Cheep[] { cheep });
            }
            
        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
        }
    }

    static void Observe(string message)
    {
        Cheep cheep = new Cheep(Environment.UserName, message, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        try
        {
            database.Store(cheep);
            UserInterface.DisplaySuccess();
        }
        catch (Exception e)
        {
           UserInterface.DisplayWriteError(e);
        }
    }
}