using CommandLine;
using SimpleDB;
using System.Net.Http.Json;

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Bison.Tests")]
namespace Bison.CLI;


/*
    * Program.cs
    * 
    * This is the entry point for the Bison CLI application.
    * It parses command line arguments and invokes the appropriate methods in BisonCliService.
*/
public class Program
{
    internal static HttpClient client = new() { BaseAddress = new Uri("http://localhost:5204") };

    public static bool Comment(string message, long observationId)
    {
        return new BisonCliService(client).Comment(message, observationId);
    }

    public static bool Propose(string taxonId, long observationId)
    {
        return new BisonCliService(client).Propose(taxonId, observationId);
    }

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<ObserveOptions, CommentOptions, ProposeOptions, ReadOptions, DiscussionOptions>(args).MapResult(
                (ObserveOptions options) =>
                {
                    new BisonCliService(client).Observe(options.Observation, options.Location);
                    return 0;
                },

                (CommentOptions options) =>
                {
                    new BisonCliService(client).Comment(string.Join(" ", options.Comment), options.ObsID);
                    return 0;
                },

                (ProposeOptions options) =>
                {
                    new BisonCliService(client).Propose(options.TaxonID, options.ObsID);
                    return 0;
                },

                (ReadOptions options) =>
                {
                    new BisonCliService(client).Read();
                    return 0;
                },

                (DiscussionOptions options) =>
                {
                    new BisonCliService(client).Discussion(options.ObsID);
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
}
