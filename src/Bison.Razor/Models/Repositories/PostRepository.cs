using Bison.Razor.Models;
using Microsoft.EntityFrameworkCore;
using Bison.Razor.Verified;

public class PostRepository : IPostRepository
{
    private readonly BisonDbContext _context;

    public PostRepository(BisonDbContext context)
    {
        _context = context;
    }

    public List<Observation> GetObservations(int pageSize, int page)
    {
        return Page(_context.Observations, pageSize, page);
    }

    public List<Observation> GetObservationsByAuthor(string authorName, int pageSize, int page)
    {
        return Page(_context.Observations.Where(o => o.Author.Name == authorName), pageSize, page);
    }

    public List<Observation> GetObservationsByTaxon(int taxonId, int pageSize, int page)
{
    return FilterByTaxon(taxonId)
        .OrderByDescending(o => o.TimeStamp)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();
}

    public int GetObservationCountByTaxon(int taxonId) => FilterByTaxon(taxonId).Length;

    private Observation[] FilterByTaxon(int taxonId)
    {
        // Load all taxa so EF Core fills in the whole Parent chain, which isSubTaxon needs
        var taxa = _context.Taxons.ToList();
        var root = taxa.FirstOrDefault(t => t.TaxonId == taxonId);
        if (root == null) return [];

        // Only observations with a taxon, since getTaxon() promises non-null
        var observations = _context.Observations
            .Include(o => o.Author)
            .Where(o => o.TaxonId != null)
            .ToArray();

        return TaxonFilter.FilterBy(root, observations);
}

    private static List<Observation> Page(IQueryable<Observation> query, int pageSize, int page)
    {
        return query
            .Include(o => o.Author)
            .OrderByDescending(o => o.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public int GetObservationCount() => _context.Observations.Count();

    public int GetObservationCountByAuthor(string authorName) =>
        _context.Observations.Count(o => o.Author.Name == authorName);

    public Observation? GetObservationById(int id)
    {
        return _context.Observations
            .Include(o => o.Author)
            .Include(o => o.Comments).ThenInclude(c => c.Author)
            .Include(o => o.Proposals).ThenInclude(p => p.Author)
            .FirstOrDefault(o => o.PostId == id);
    }

    public List<Comment> GetCommentsForObservation(int observationId)
    {
        return _context.Comments
            .Where(c => c.ObservationId == observationId)
            .Include(c => c.Author)
            .OrderBy(c => c.TimeStamp)
            .ToList();
    }

    public List<Proposal> GetProposalsForObservation(int observationId)
    {
        return _context.Proposals
            .Where(p => p.ObservationId == observationId)
            .Include(p => p.Author)
            .OrderBy(p => p.TimeStamp)
            .ToList();
    }

    public void AddObservation(string authorName, string text)
    {
        var author = _context.Authors.FirstOrDefault(a => a.Name == authorName);
        if (author is null) return; // samme adfærd som før

        _context.Observations.Add(new Observation { Author = author, Text = text, TimeStamp = DateTime.UtcNow });
        _context.SaveChanges();
    }
}