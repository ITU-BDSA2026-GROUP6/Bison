using CommandLine;
using SimpleDB;

public record Cheep(string Author, string Observation, long Timestamp);

[Verb("observe", HelpText = "Store a new observation.")]
public class ObserveOptions
{
    [Value(0, MetaName = "observation", Required = true, HelpText = "The observation text to store.")]
    public IEnumerable<string> Observation { get; set; } = [];
}

class Program
{
    static readonly IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(Path.Combine("CSVfiles", "bison_observe_cli_db.csv"));
    
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments(args, typeof(ObserveOptions))
            .WithParsed<ObserveOptions>(options => Observe(string.Join(" ", options.Observation)))
            .WithNotParsed(DisplayParseError);
    }

    static void DisplayParseError(IEnumerable<Error> errors)
    {
        bool helpOrVersionRequested = errors.Any(error =>
            error is HelpRequestedError ||
            error is HelpVerbRequestedError ||
            error is VersionRequestedError);

        if (!helpOrVersionRequested)
        {
            UserInterface.DisplayInvalidCommandMessage();
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

        DisplayStoredObservations();
    }

    static void DisplayStoredObservations()
    {
        try
        {
            UserInterface.DisplayObservations(database.Read());
        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
        }
    }
}
