using CommandLine;
using SimpleDB;
using System.Net.Http.Json;

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Bison.Tests")]
namespace Bison.CLI;

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
    internal static HttpClient client = new() { BaseAddress = new Uri("http://localhost:5204") };

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
            client.PostAsJsonAsync("/observation", observation).Result.EnsureSuccessStatusCode();
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
            var observations = client.GetFromJsonAsync<List<Observation>> ("/observations").Result!;
            //The field above does three things. Sends the HTTP GET to http://localhost:5204/observations
            //Receives the JSON the web service returns.
            //Converts that JSON into a List<Observation>
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
            var observations = client.GetFromJsonAsync<List<Observation>> ("/observations").Result!;

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
            var observations = client.GetFromJsonAsync<List<Observation>> ("/observations").Result!;
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
            //commentDatabase.Store(comment);
            client.PostAsJsonAsync("/comment", comment).Result.EnsureSuccessStatusCode();
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
            var observation = client.GetFromJsonAsync<List<Observation>> ("/observations").Result!
            .FirstOrDefault(o => o.ObsID == obsID);
            if (observation == null)
            {
                System.Console.WriteLine($"Observation with ID {obsID} does not exist.");
                return;
            }
            var comments = client.GetFromJsonAsync<List<Comment>>($"/comments?id={obsID}").Result!;
            UserInterface.DisplayDiscussion(observation, comments);
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
            var observations = client.GetFromJsonAsync<List<Observation>> ("/observations").Result!;
            UserInterface.DisplayObservations(observations);
        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
        }
    }
}

