using CommandLine;

namespace Bison.CLI;

/*
    * Commands.cs
    * 
    * This file contains the command line options for the Bison CLI application.
    * Each class defines the arguments and options for one CLI command.
*/

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

[Verb("propose", HelpText = "Propose a taxon for an observation.")]
public class ProposeOptions
{
    [Value(0, MetaName = "taxon-id", Required = true,
        HelpText = "The taxon ID to propose.")]
    public string TaxonID { get; set; } = "";

    [Value(1, MetaName = "observation-id", Required = true,
        HelpText = "The ID of the observation to propose a taxon for.")]
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