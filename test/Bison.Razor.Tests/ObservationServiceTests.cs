namespace Bison.Razor.Tests;

using Bison.Razor.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class ObservationServiceTests
{
    private const string UserId = "adrian-account-id";
    private readonly ObservationService _service;
    private readonly BisonDbContext _context;

    public ObservationServiceTests()
    {
        // In-memory SQLite, lives as long as the connection is open
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<BisonDbContext>().UseSqlite(connection).Options;
        var context = new BisonDbContext(options);
        context.Database.EnsureCreated();

        // Minimal test data, replace with DbInitializer once 1d is done
        var user = new BisonUser
        {
            Id = UserId, UserName = "Adrian", Email = "adrian@example.com"
        };
        context.Users.Add(user);
        var adrian = new Author
        {
            Name = "Adrian", Email = "adrian@example.com", BisonUserId = user.Id
        };
        context.Observations.Add(new Observation { Author = adrian, Text = "Saw a heron", TimeStamp = DateTime.UtcNow });
        context.SaveChanges();
        _context = context;

        var repo = new PostRepository(context);
        var db = new DBFacade(repo);
        _service = new ObservationService(db);
    }

    [Fact]
    public void GetObservationById_ReturnsNull_WhenObservationDoesNotExist()
    {
        var result = _service.GetObservationById(-1);

        Assert.Null(result);
    }

    [Fact]
    public void GetObservations_ReturnsNonEmptyList_ForFirstPage()
    {
        var result = _service.GetObservations(page: 1);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetObservationsFromAuthor_ReturnsOnlyThatAuthorsObservations()
    {
        var result = _service.GetObservationsFromAuthor("Adrian", page: 1);

        Assert.NotEmpty(result);
        Assert.All(result, obs => Assert.Equal("Adrian", obs.Author));
    }

    [Fact]
    public void GetCommentsForObservation_ReturnsEmptyList_WhenObservationDoesNotExist()
    {
        var result = _service.GetCommentsForObservation(-1);

        Assert.Empty(result);
    }

    [Fact]
    public void GetProposalsForObservation_ReturnsEmptyList_WhenObservationDoesNotExist()
    {
        var result = _service.GetProposalsForObservation(-1);

        Assert.Empty(result);
    }

    [Fact]
    public void AddComment_ThenGetCommentsForObservation_ReturnsNewComment()
    {
        var observationId = _service.GetObservations(page: 1).First().ObservationId;

        _service.AddComment(UserId, observationId, "Nice spot!");

        var comments = _service.GetCommentsForObservation(observationId);
        Assert.Contains(comments, c => c.Message == "Nice spot!");
    }

    [Fact]
    public void AddProposal_ThenGetProposalsForObservation_ReturnsNewProposal()
    {
        var observationId = _service.GetObservations(page: 1).First().ObservationId;

        var taxon = new Taxon { dwc_TaxonID = "test-taxon", VernacularName = "Heron" };
        _context.Taxons.Add(taxon);
        _context.SaveChanges();

        _service.AddProposal(UserId, observationId, taxon.TaxonId, "Should be a heron");

        var proposals = _service.GetProposalsForObservation(observationId);
        Assert.Contains(proposals, p => p.Message == "Should be a heron");
    }
}
