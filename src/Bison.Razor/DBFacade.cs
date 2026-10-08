using Bison.Razor.Models;

public class DBFacade
{
    private const string DateFormat = "MM/dd/yy H:mm:ss";
    private readonly IPostRepository _repo;

    public DBFacade(IPostRepository repo)
    {
        _repo = repo;
    }

    public List<ObservationViewModel> GetObservations(int pageSize, int page = 1)
    {
        return _repo.GetObservations(pageSize, page).Select(ToViewModel).ToList();
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int pageSize, int page = 1)
    {
        return _repo.GetObservationsByAuthor(author, pageSize, page).Select(ToViewModel).ToList();
    }

    public int GetObservationCount() => _repo.GetObservationCount();

    public int GetObservationCountFromAuthor(string author) => _repo.GetObservationCountByAuthor(author);

    public List<CommentViewModel> GetCommentsForObservation(int observationId)
    {
        return _repo.GetCommentsForObservation(observationId)
            .Select(c => new CommentViewModel(c.Author.Name, c.Text, c.TimeStamp.ToString(DateFormat)))
            .ToList();
    }

    public List<ProposalViewModel> GetProposalsForObservation(int observationId)
    {
        return _repo.GetProposalsForObservation(observationId)
            .Select(p => new ProposalViewModel(p.Author.Name, p.Text, p.TimeStamp.ToString(DateFormat)))
            .ToList();
    }

    public void AddObservation(string author, string text)
    {
        _repo.AddObservation(author, text);
    }

    public ObservationViewModel? GetObservationById(int id)
    {
        var o = _repo.GetObservationById(id);
        return o is null ? null : ToViewModel(o);
    }

    private static ObservationViewModel ToViewModel(Observation o) =>
        new(o.PostId, o.Author.Name, o.Text, o.TimeStamp.ToString(DateFormat));
}