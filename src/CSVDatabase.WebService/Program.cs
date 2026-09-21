using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var taxonomy = Taxonomy.LoadEmbedded();

string csvDirectory = Path.Combine(AppContext.BaseDirectory, "CSVFiles");
Directory.CreateDirectory(csvDirectory);

IDatabaseRepository<Observation> observationDatabase =
    CSVDatabase<Observation>.GetInstance(
        Path.Combine(csvDirectory, "observations.csv"));

IDatabaseRepository<Comment> commentDatabase =
    CSVDatabase<Comment>.GetInstance(
        Path.Combine(csvDirectory, "comments.csv"));

IDatabaseRepository<Proposal> proposalDatabase =
    CSVDatabase<Proposal>.GetInstance(
        Path.Combine(csvDirectory, "proposals.csv"));

app.MapPost("/observation", (Observation observation) =>
{
    observationDatabase.Store(observation);
    return Results.Ok();
});

app.MapPost("/comment", (Comment comment) =>
{
    commentDatabase.Store(comment);
    return Results.Ok();
});

app.MapPost("/proposal", (Proposal proposal) =>
{
    proposalDatabase.Store(proposal);
    return Results.Ok();
});

app.MapGet("/observations", () =>
{
    return Results.Ok(observationDatabase.Read());
});

app.MapGet("/comments", (long id) =>
{
    var comments = commentDatabase
        .Read()
        .Where(comment => comment.ObsID == id);

    return Results.Ok(comments);
});

app.MapGet("/proposals", (long id) =>
{
    var proposals = proposalDatabase
        .Read()
        .Where(proposal => proposal.ObsID == id);

    return Results.Ok(proposals);
});

app.Run();
public partial class Program { }