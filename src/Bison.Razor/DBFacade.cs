using Bison.Razor.Models;

public class DBFacade
{
    private const string DateFormat = "MM/dd/yy H:mm:ss";
    private readonly BisonDbContext _context;

    public DBFacade(BisonDbContext context)
    {
        _context = context;
    }

    public List<ObservationViewModel> GetObservations(int pageSize, int page = 1) //page = 1 means its a default value so it shows page 1 first
    {
        return Page(_context.Observations, pageSize, page);
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int pageSize, int page = 1)
    {
        return Page(_context.Observations.Where(o => o.Author.Name == author), pageSize, page);
    }

    public int GetObservationCount()
    {
        return _context.Observations.Count();
    }

    public int GetObservationCountFromAuthor(string author)
    {
        return _context.Observations.Count(o => o.Author.Name == author);
    }

    public List<CommentViewModel> GetCommentsForObservation(int observationId)
    {
        return _context.Comments
            .Where(c => c.ObservationId == observationId)
            .OrderBy(c => c.TimeStamp)
            .Select(c => new CommentViewModel(c.Author.Name, c.Text, c.TimeStamp.ToString(DateFormat)))
            .ToList();
    }

    // Newest first, paginated
    private static List<ObservationViewModel> Page(IQueryable<Observation> query, int pageSize, int page)
    {
        return query
            .OrderByDescending(o => o.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new ObservationViewModel(o.PostId, o.Author.Name, o.Text, o.TimeStamp.ToString(DateFormat)))
            .ToList();
    }

    public List<ProposalViewModel> GetProposalsForObservation(int observationId)
    {
        return _context.Proposals
            .Where(p => p.ObservationId == observationId)
            .OrderBy(p => p.TimeStamp)
            .Select(p => new ProposalViewModel(p.Author.Name, p.Text, p.TimeStamp.ToString(DateFormat)))
            .ToList();
    }

    public void AddObservation(String author, string text)
    {
        // Unknown author inserts nothing, same as the old SQL
        var a = _context.Authors.FirstOrDefault(a => a.Name == author);
        if (a is null) return;

        _context.Observations.Add(new Observation { Author = a, Text = text, TimeStamp = DateTime.UtcNow });
        _context.SaveChanges();
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

    public ObservationViewModel? GetObservationById(int id)
    {
        return _context.Observations
            .Where(o => o.PostId == id)
            .Select(o => new ObservationViewModel(o.PostId, o.Author.Name, o.Text, o.TimeStamp.ToString(DateFormat)))
            .FirstOrDefault();
    }
}
