using System.Globalization;
using CsvHelper;

namespace SimpleDB;

public class Taxonomy
{
    private readonly List<Taxon> taxa;

    public Taxonomy(IEnumerable<Taxon> taxa)
    {
        this.taxa = taxa.ToList();
    }

    public static Taxonomy LoadEmbedded()
    {
        using var stream = typeof(Taxonomy).Assembly
            .GetManifestResourceStream("SimpleDB.Taxonomy.csv")
            ?? throw new InvalidOperationException(
                "The embedded taxonomy CSV was not found.");

        using StreamReader reader = new(stream);
        using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

        List<Taxon> records = csv.GetRecords<Taxon>().ToList();

        return new Taxonomy(records);
    }

    public Taxon? GetById(string id)
    {
        return taxa.FirstOrDefault(taxon => taxon.TaxonID == id);
    }

    public IEnumerable<Taxon> GetByDanishName(string name)
    {
        return taxa.Where(taxon =>
            taxon.Language == "dan" &&
            !string.IsNullOrWhiteSpace(taxon.VernacularName) &&
            string.Equals(
                taxon.VernacularName.Trim(),
                name.Trim(),
                StringComparison.OrdinalIgnoreCase));
    }

    public Taxon? GetParent(string id)
    {
        Taxon? taxon = GetById(id);

        if (taxon == null ||
            string.IsNullOrWhiteSpace(taxon.ParentNameUsageID))
        {
            return null;
        }

        return GetById(taxon.ParentNameUsageID);
    }

    public IEnumerable<Taxon> GetChildren(string id)
    {
        return taxa.Where(taxon => taxon.ParentNameUsageID == id);
    }
}