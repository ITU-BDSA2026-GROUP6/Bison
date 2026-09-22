using SimpleDB;

namespace CSVDatabase.Tests;

public class TaxonomyTests
{
    [Fact]
    //test to check if the GetById function returns the expected taxon
    public void GetById_ReturnsExpectedTaxon()
    {
        var taxonomy = Taxonomy.LoadEmbedded();
        string id = "MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea";

        var taxon = taxonomy.GetById(id);

        Assert.NotNull(taxon);
        Assert.Equal("Ardea cinerea", taxon!.ScientificName);
    }

    [Fact]
    //test to check if the GetByDanishName function returns the expected taxon
    public void GetByDanishName_ReturnsExpectedTaxon()
    {
        var taxonomy = Taxonomy.LoadEmbedded();

        var taxon = Assert.Single(
            taxonomy.GetByDanishName("Fiskehejre"));

        Assert.Equal("Ardea cinerea", taxon.ScientificName);
    }

    [Fact]
    //test to check if the GetParent function returns the direct parent of a taxon
    public void GetParent_ReturnsDirectParent()
    {
        var taxonomy = Taxonomy.LoadEmbedded();
        var heron = Assert.Single(
            taxonomy.GetByDanishName("Fiskehejre"));

        var parent = taxonomy.GetParent(heron.TaxonID);

        Assert.NotNull(parent);
        Assert.Equal("Ardea", parent!.ScientificName);
    }

    [Fact]
    //test to check if the GetChildren function returns the direct children of a taxon
    public void GetChildren_ReturnsDirectChildren()
    {
        var taxonomy = Taxonomy.LoadEmbedded();
        var family = Assert.Single(
            taxonomy.GetByDanishName("Hejrer"));

        var children = taxonomy.GetChildren(family.TaxonID).ToList();

        Assert.Equal(7, children.Count);
        Assert.Contains(children, taxon => taxon.ScientificName == "Ardea");
        Assert.All(children, taxon => Assert.Equal("genus", taxon.TaxonRank));
    }

    [Fact]
    //test to check if the GetParent function returns null when the parent is outside the dataset
    public void GetParent_ReturnsNull_WhenParentIsOutsideDataset()
    {
        var taxonomy = Taxonomy.LoadEmbedded();
        string orderId =
            "MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea";

        Assert.NotNull(taxonomy.GetById(orderId));
        Assert.Null(taxonomy.GetParent(orderId));
    }

    [Fact]
    //test to check if  it does not return any results when the taxon does not exist
    public void UnknownTaxon_ReturnsNoResults()
    {
        var taxonomy = Taxonomy.LoadEmbedded();

        Assert.Null(taxonomy.GetById("unknown-id"));
        Assert.Null(taxonomy.GetParent("unknown-id"));
        Assert.Empty(taxonomy.GetChildren("unknown-id"));
        Assert.Empty(taxonomy.GetByDanishName("unknown-name"));
    }
}