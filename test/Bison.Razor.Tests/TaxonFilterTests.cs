namespace Bison.Razor.Tests;

using Bison.Razor.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class TaxonFilterTests
{
    private readonly PostRepository _repo;
    private readonly int _mammaliaId, _bovidaeId, _avesId, _bisonId;

    public TaxonFilterTests()
    {
        // In-memory SQLite, lives as long as the connection is open
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<BisonDbContext>().UseSqlite(connection).Options;
        var context = new BisonDbContext(options);
        context.Database.EnsureCreated();

        // Taxon tree: Mammalia -> Bovidae -> Bison, and Aves on its own
        var mammalia = new Taxon { dwc_TaxonID = "mammalia", VernacularName = "Mammals" };
        var bovidae = new Taxon { dwc_TaxonID = "bovidae", VernacularName = "Bovids", Parent = mammalia };
        var bison = new Taxon { dwc_TaxonID = "bison", VernacularName = "Bison", Parent = bovidae };
        var aves = new Taxon { dwc_TaxonID = "aves", VernacularName = "Birds" };

        var adrian = new Author { Name = "Adrian", Email = "adrian@example.com" };
        context.Observations.AddRange(
            new Observation { Author = adrian, Text = "Saw a bison", TimeStamp = DateTime.UtcNow, Taxon = bison },
            new Observation { Author = adrian, Text = "Saw some bovid", TimeStamp = DateTime.UtcNow, Taxon = bovidae },
            new Observation { Author = adrian, Text = "Saw a mammal", TimeStamp = DateTime.UtcNow, Taxon = mammalia },
            new Observation { Author = adrian, Text = "Saw a bird", TimeStamp = DateTime.UtcNow, Taxon = aves },
            new Observation { Author = adrian, Text = "Saw something", TimeStamp = DateTime.UtcNow });
        context.SaveChanges();

        _mammaliaId = mammalia.TaxonId;
        _bovidaeId = bovidae.TaxonId;
        _avesId = aves.TaxonId;
        _bisonId = bison.TaxonId;

        // Forget the objects created above, so the repository has to load everything from the database
        context.ChangeTracker.Clear();

        _repo = new PostRepository(context);
    }

    [Fact]
    public void GetObservationsByTaxon_IncludesAllSubtaxa()
    {
        var result = _repo.GetObservationsByTaxon(_mammaliaId, pageSize: 10, page: 1);

        Assert.Equal(
            ["Saw a bison", "Saw a mammal", "Saw some bovid"],
            result.Select(o => o.Text).Order());
    }

    [Fact]
    public void GetObservationsByTaxon_ExcludesParentTaxa()
    {
        var result = _repo.GetObservationsByTaxon(_bovidaeId, pageSize: 10, page: 1);

        Assert.Equal(["Saw a bison", "Saw some bovid"], result.Select(o => o.Text).Order());
    }

    [Fact]
    public void GetObservationsByTaxon_ExcludesUnrelatedTaxa()
    {
        var result = _repo.GetObservationsByTaxon(_avesId, pageSize: 10, page: 1);

        Assert.Equal(["Saw a bird"], result.Select(o => o.Text));
    }

    [Fact]
    public void GetObservationsByTaxon_LeafTaxonReturnsOnlyItself()
    {
        var result = _repo.GetObservationsByTaxon(_bisonId, pageSize: 10, page: 1);

        Assert.Equal(["Saw a bison"], result.Select(o => o.Text));
    }

    [Fact]
    public void GetObservationsByTaxon_ReturnsEmpty_WhenTaxonDoesNotExist()
    {
        var result = _repo.GetObservationsByTaxon(-1, pageSize: 10, page: 1);

        Assert.Empty(result);
    }

    [Fact]
    public void GetObservationCountByTaxon_CountsSubtaxa()
    {
        Assert.Equal(3, _repo.GetObservationCountByTaxon(_mammaliaId));
    }
}
