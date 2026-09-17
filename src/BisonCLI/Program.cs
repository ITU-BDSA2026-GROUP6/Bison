using CommandLine;
using SimpleDB;

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Bison.Tests")]


[Verb("observe", HelpText = "Store a new observation.")]
public class ObserveOptions
{
    [Value(0, MetaName = "observation", Required = true,
        HelpText = "The observation text to store.")]
    public string Observation { get; set; } = "";

    [Value(1, MetaName = "location", Required = true,
        HelpText = "The location of the observation.")]
    public string Location { get; set; } = "";
}

[Verb("comment", HelpText = "Store a new comment.")]
public class CommentOptions
{
    [Value(0, MetaName = "comment", Required = true, Max = 1,
        HelpText = "The comment text.")]
    public IEnumerable<string> Comment { get; set; } = [];

    [Value(1, MetaName = "observation-id", Required = true,
        HelpText = "The ID of the observation to comment on.")]
    public long ObsID { get; set; }
}

[Verb("read", HelpText = "Display all observations.")]
public class ReadOptions
{
}

[Verb("discussion", HelpText = "Display an observation and its comments.")]
public class DiscussionOptions
{
    [Value(0, MetaName = "observation-id", Required = true,
        HelpText = "The ID of the observation.")]
    public long ObsID { get; set; }
}

public class Program
{
    internal static IDatabaseRepository<Observation> database =
    CSVDatabase<Observation>.GetInstance(Path.Combine("CSVfiles", "bison_observe_cli_db.csv"));
    internal static IDatabaseRepository<Comment> commentDatabase =
    CSVDatabase<Comment>.GetInstance(Path.Combine("CSVfiles", "bison_comment_cli_db.csv"));


    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<ObserveOptions, CommentOptions, ReadOptions, DiscussionOptions>(args).MapResult(
                (ObserveOptions options) =>
                {
                    Observe(options.Observation, options.Location);
                    return 0;
                },

                (CommentOptions options) =>
                {
                    Comment(string.Join(" ", options.Comment), options.ObsID);
                    return 0;
                },

                (ReadOptions options) =>
                {
                    Read();
                    return 0;
                },

                (DiscussionOptions options) =>
                {
                    Discussion(options.ObsID);
                    return 0;
                },

                errors =>
                {
                    DisplayParseError(errors);
                    return 1;
                });
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

    static void Observe(string message, string location)
    {
        Observation observation = new Observation(
            GetNextObservationID(),
            Environment.UserName,
            message,
            location,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        try
        {
            database.Store(observation);
            UserInterface.DisplaySuccess();
        }
        catch (Exception e)
        {
            UserInterface.DisplayWriteError(e);
        }

        DisplayStoredObservations();
    }

    static long GetNextObservationID()
    {
        try
        {
            var observations = database.Read().ToList();

            return observations.Count + 1;
        }
        catch
        {
            return 1;
        }
    }
    static void Read()
    {
        try
        {
            var observations = database.Read();

            UserInterface.DisplayObservations(observations);
        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
        }
    }

    public static bool Comment(string message, long obsID)
    {
        try
        {
            var observations = database.Read();
            bool observationExists = observations.Any(o => o.ObsID == obsID);

            if (!observationExists)
            {
                System.Console.WriteLine($"Observation with ID {obsID} does not exist.");
                return false;
            }
            Comment comment = new Comment(
                obsID,
                Environment.UserName,
                message,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            commentDatabase.Store(comment);
            UserInterface.DisplaySuccess();
            return true;

        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
            return false;
        }
    }

    static void Discussion(long obsID)
    {
        try
        {
            var observations = database.Read().FirstOrDefault(o => o.ObsID == obsID);
            if (observations == null)
            {
                System.Console.WriteLine($"Observation with ID {obsID} does not exist.");
                return;
            }
            var comments = commentDatabase.Read().Where(c => c.ObsID == obsID);
            UserInterface.DisplayDiscussion(observations, comments);
        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
            return;
        }
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

