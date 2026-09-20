using CsvHelper.Configuration.Attributes;

namespace SimpleDB;

public class Taxon
{
    [Name("dwc:taxonID")]
    public string TaxonID { get; set; } = "";

    [Name("dwc:parentNameUsageID")]
    public string? ParentNameUsageID { get; set; }

    [Name("dwc:acceptedNameUsageID")]
    public string? AcceptedNameUsageID { get; set; }

    [Name("dwc:taxonomicStatus")]
    public string TaxonomicStatus { get; set; } = "";

    [Name("dwc:taxonRank")]
    public string TaxonRank { get; set; } = "";

    [Name("dwc:scientificName")]
    public string ScientificName { get; set; } = "";

    [Name("dwc:scientificNameAuthorship")]
    public string? ScientificNameAuthorship { get; set; }

    [Name("dcterms:language")]
    public string? Language { get; set; }

    [Name("dwc:vernacularName")]
    public string? VernacularName { get; set; }

    [Name("clb:merged")]
    public bool? Merged { get; set; }
}