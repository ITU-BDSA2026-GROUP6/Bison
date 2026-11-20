using Bison.Razor.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

public sealed class ObserveIntegrationTests
{
    [Fact]
    public void AddObservation_WithSelectedTaxon_PersistsTheObservation()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        context.Database.EnsureCreated();
        context.Taxons.Add(new Taxon { TaxonId = 7, VernacularName = "Heron" });
        context.SaveChanges();

        var service = new ObservationService(new DBFacade(new PostRepository(context)));
        service.AddObservation("Adrian", "Saw a heron", 7);

        var savedObservation = Assert.Single(context.Observations);
        Assert.Equal("Saw a heron", savedObservation.Text);
        Assert.Equal(7, savedObservation.TaxonId);
    }

    [Fact]
    public void AddObservation_WithUnknownTaxon_ReturnsNoSavedObservation()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        context.Database.EnsureCreated();
        var service = new ObservationService(new DBFacade(new PostRepository(context)));
        var taxons = service.GetTaxons();

        Assert.DoesNotContain(taxons, taxon => taxon.TaxonId == 999);
        Assert.Empty(context.Observations);
    }

    private static BisonDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<BisonDbContext>()
            .UseSqlite(connection)
            .Options;
        return new BisonDbContext(options);
    }
}
