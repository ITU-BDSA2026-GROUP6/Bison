using Bison.Domain.Models;
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

    private Author GetAuthor(string userId)
    {
        var user = _context.Users.SingleOrDefault(u => u.Id == userId)
         ?? throw new InvalidOperationException("Account not found.");

        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            throw new InvalidOperationException("Account has no username.");
        }

        return user;
    }

    public List<Observation> GetObservationsByAuthor(string authorName, int pageSize, int page)
    {
        return Page(_context.Observations.Where(o => o.Author.UserName! == authorName), pageSize, page);
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
            .Include(o => o.Likes)
            .OrderByDescending(o => o.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public int GetObservationCount() => _context.Observations.Count();

    public int GetObservationCountByAuthor(string authorName) =>
        _context.Observations.Count(o => o.Author.UserName! == authorName);

        public Observation? GetObservationById(int id)
    {
        return _context.Observations
            .Include(o => o.Author)
            .Include(o => o.Likes)
            .Include(o => o.Taxon)
            .Include(o => o.Picture)
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
            .Include(p => p.Taxon)
            .OrderBy(p => p.TimeStamp)
            .ToList();
    }

    public void AddObservation(string userId, string text, int? taxonId = null, Picture? picture = null)
    {
        var author = GetAuthor(userId);

        var taxon = _context.Taxons.Find(taxonId);
        if (taxon is null && taxonId.HasValue)
        {
            throw new ArgumentException($"Taxon with ID {taxonId} does not exist.");
        }

        _context.Observations.Add(new Observation { Author = author, Text = text, TimeStamp = DateTime.UtcNow, TaxonId = taxonId, Picture = picture });
        _context.SaveChanges();
    }

    public void AddComment(string userId, int observationId, string text)
    {
        var author = GetAuthor(userId);

        var observation = _context.Observations.Find(observationId);
        if (observation is null)
        {
            throw new ArgumentException($"Observation with ID {observationId} does not exist.");
        }

        _context.Comments.Add(new Comment
        {
            Author = author,
            ObservationId = observationId,
            Text = text,
            TimeStamp = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    public void AddProposal(string userId, int observationId, int taxonId, string text)
    {
        var author = GetAuthor(userId);

        var observation = _context.Observations.Find(observationId);
        if (observation is null)
        {
            throw new ArgumentException($"Observation with ID {observationId} does not exist.");
        }

        var taxon = _context.Taxons.Find(taxonId);
        if (taxon is null)
        {
            throw new ArgumentException($"Taxon with ID {taxonId} does not exist.");
        }

        _context.Proposals.Add(new Proposal
        {
            Author = author,
            ObservationId = observationId,
            TaxonId = taxonId,
            Text = text,
            TimeStamp = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    public void Like (string userId, int postId, bool isLike)
    {
        var author = GetAuthor(userId);
        if (_context.Posts.Find(postId) is null)
            throw new ArgumentException($"Post with ID {postId} does not exist.");
        
        var like = _context.Likes.Find(postId, author.Id);
        if (like is null)
            _context.Likes.Add(new Like { PostId = postId, Author = author, IsLike = isLike });
        else if (like.IsLike == isLike)
            _context.Likes.Remove(like);
        else
        
            like.IsLike = isLike;
        
        _context.SaveChanges();
    }

    public List<Taxon> GetTaxons()
    {
        return _context.Taxons.ToList();
    }


    public PersonalData? GetPersonalData(string userId)
    {
        var user = _context.Users
            .AsNoTracking()
            .SingleOrDefault(u => u.Id == userId);

        if (user is null)
        {
            return null;
        }

        var posts = _context.Observations
            .AsNoTracking()
            .Include(o => o.Taxon)
            .Include(o => o.Picture)
            .Where(o => o.AuthorId == userId)
            .OrderByDescending(o => o.TimeStamp)
            .ToList();

        var comments = _context.Comments
            .AsNoTracking()
            .Where(c => c.AuthorId == userId)
            .OrderByDescending(c => c.TimeStamp)
            .ToList();

        var proposals = _context.Proposals
            .AsNoTracking()
            .Include(p => p.Taxon)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.TimeStamp)
            .ToList();

        return new PersonalData(
            user.Id,
            user.UserName,
            user.Email ?? string.Empty,
            posts,
            comments,
            proposals);
    }
}
