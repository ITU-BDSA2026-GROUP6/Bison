using Microsoft.EntityFrameworkCore;
public record ObservationViewModel(int ObservationId, string Author, string Message, string Timestamp);
public record CommentViewModel(string Author, string Message, string Timestamp);
public record ProposalViewModel(string Author, string Message, string Timestamp)
{
    public object TaxonId { get; internal set; }
}

public interface IObservationService
{
    public List<ObservationViewModel> GetObservations(int page);
    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int page);
    public int GetObservationCount();
    public int GetObservationCountFromAuthor(string author);

        ObservationViewModel? GetObservationById(int id);
        List<CommentViewModel> GetCommentsForObservation(int observationId);
        List<ProposalViewModel> GetProposalsForObservation(int observationId);
}

public class ObservationService : IObservationService
{
    public const int PageSize = 32; 
    private readonly DBFacade _db;
    public ObservationService(DBFacade db)
    {
        _db = db;
    }

    public List<ObservationViewModel> GetObservations(int page)
    {
        return _db.GetObservations(PageSize, page);
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int page)
    {
        // filter by the provided author name
        return _db.GetObservationsFromAuthor(author, PageSize, page);
    }

    public int GetObservationCount()
    {
        return _db.GetObservationCount();
    }

    public int GetObservationCountFromAuthor(string author)
    {
        return _db.GetObservationCountFromAuthor(author);
    }

    public void AddObservation(string author, string message)
    {
        _db.AddObservation(author, message);
    }

    public ObservationViewModel? GetObservationById(int id)
    {
        return _db.GetObservationById(id);
    }

    public List<CommentViewModel> GetCommentsForObservation(int observationId)
    {
        return _db.GetCommentsForObservation(observationId);
    }

    public List<ProposalViewModel> GetProposalsForObservation(int observationId)    
    {
        return _db.GetProposalsForObservation(observationId);
    }
}
