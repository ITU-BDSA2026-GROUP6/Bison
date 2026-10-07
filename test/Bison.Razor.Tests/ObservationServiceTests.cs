namespace Bison.Razor.Tests;

using Bison.Razor.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class ObservationServiceTests
{
    private readonly ObservationService _service;

        public ObservationServiceTests()
    {
        // In-memory SQLite, lives as long as the connection is open
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<BisonDbContext>().UseSqlite(connection).Options;
        var context = new BisonDbContext(options);
        context.Database.EnsureCreated();

        // Minimal test data, replace with DbInitializer once 1d is done
        var adrian = new Author { Name = "Adrian", Email = "adrian@example.com" };
        context.Observations.Add(new Observation { Author = adrian, Text = "Saw a heron", TimeStamp = DateTime.UtcNow });
        context.SaveChanges();

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
}