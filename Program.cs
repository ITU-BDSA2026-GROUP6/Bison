using Commandline;
using SimpleDB;

public class Cheep
{
    public string Author { get; }
    public string Text { get; }
    public long Timestamp { get; }

    public Cheep(string author, string text, long timestamp)
    {
        Author = author;
        Text = text;
        Timestamp = timestamp;
    }
}
public class Observation : Cheep
{
    public long ObsID { get; }

    public Observation(
        long obsID,
        string author,
        string text,
        long timestamp)
        : base(author, text, timestamp)
    {
        ObsID = obsID;
    }
}
public class Comment : Cheep
{   
    public long ObsID { get; }

    public Comment(
        long obsID,
        string author,
        string text,
        long timestamp)
        : base(author, text, timestamp)
    {
        ObsID = obsID;
    }
}

[Verb("observe", HelpText = "Store a new observation.")]
public class ObserveOptions
{
    [Value(0, MetaName = "observation", Required = true,
        HelpText = "The observation text to store.")]
    public IEnumerable<string> Observation { get; set; } = [];
}

[Verb("comment", HelpText = "Store a new comment.")]
public class CommentOptions
{
    [Value(0, MetaName = "comment", Required = true,
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

class Program
{
    static long nextObservationID = 1;
    static readonly IDatabaseRepository<Observation> database = new CSVDatabase<Observation>(Path.Combine("CSVfiles", "bison_observe_cli_db.csv"));
    static readonly IDatabaseRepository<Comment> commentDatabase = new CSVDatabase<Comment>(Path.Combine("CSVfiles", "bison_comment_cli_db.csv"));
    
    
    static void Main(string[] args)
{
    Parser.Default.ParseArguments<ObserveOptions, CommentOptions, ReadOptions, DiscussionOptions>(args).MapResult(
            (ObserveOptions options) =>
                Observe(string.Join(" ", options.Observation)),

            (CommentOptions options) =>
                Comment(
                    string.Join(" ", options.Comment),
                    options.ObsID),

            (ReadOptions options) =>
                Read(),

            (DiscussionOptions options) =>
                Discussion(options.ObsID),

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

    static void Observe(string message)
    {
        Observation observation = new Observation(
            nextObservationID,
            Environment.UserName,
            message,
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
    public static void DisplayObservations(IEnumerable<Observation> observations)
{
    foreach (Observation observation in observations)
    {
        System.Console.WriteLine(
            $"ID: {observation.ObsID}, " +
            $"Author: {observation.Author}, " +
            $"Observation: {observation.Text}, " +
            $"Timestamp: {observation.Timestamp}"
        );
    }
}
    
    static void Comment(string message, long obsID)
    {
        try
        {
            var observations = database.Read();
            bool observationExists = observations.Any(o => o.ObsID == obsID);

            if (!observationExists)
            {
                System.Console.WriteLine($"Observation with ID {obsID} does not exist.");
                return;
            }
            Comment comment = new Comment(
                obsID,
                Environment.UserName,
                message,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            commentDatabase.Store(comment);
            UserInterface.DisplaySuccess();
        
        } catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
            return;
        }
    }
    
    static void Discussion(long obsID)
    {
        try
        {
            var observations = database.Read().firstOrDefault(o => o.ObsID == obsID);
            if (observations == null)
            {
                System.Console.WriteLine($"Observation with ID {obsID} does not exist.");
                return;            
        }
        var comments = commentDatabase.Read().Where(c => c.ObsID == obsID);
        UserInterface.DisplayObservations(new List<Observation> { observations });
        UserInterface.DisplayComments(comments);
        } catch (Exception e)
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

