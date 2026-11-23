using Bison.Razor.Models;
public record ObservationViewModel(int ObservationId, string Author, string Message, string Timestamp, string? VernacularName, string? PictureSrc = null);
public record CommentViewModel(string Author, string Message, string Timestamp);
public record ProposalViewModel(string Author, string Message, string Timestamp, string? TaxonName);

public interface IObservationService
{
    public List<ObservationViewModel> GetObservations(int page);
    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int page);
    public int GetObservationCount();
    public int GetObservationCountFromAuthor(string author);

    ObservationViewModel? GetObservationById(int id);
    List<CommentViewModel> GetCommentsForObservation(int observationId);
    List<ProposalViewModel> GetProposalsForObservation(int observationId);
    void AddObservation(string userId, string message, int? taxonId = null, Picture? picture = null);

    void AddComment(string userId, int observationId, string text);
    void AddProposal(string userId, int observationId, int taxonId, string text);

    public List<Taxon> GetTaxons();

    public int GetObservationCountByTaxon(int taxonID);

    public List<ObservationViewModel> GetObservationsByTaxon(int taxonID, int currentpage);
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

    public void AddObservation(string author, string message, int? taxonId = null, Picture? picture = null)
    {
        _db.AddObservation(author, message, taxonId, picture);
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

    public void AddComment(string author, int observationId, string text)
    {
        _db.AddComment(author, observationId, text);
    }

    public void AddProposal(string author, int observationId, int taxonId, string text)
    {
        _db.AddProposal(author, observationId, taxonId, text);
    }
    public List<Taxon> GetTaxons()
    {
        return _db.GetTaxons();
    }

    public List<ObservationViewModel> GetObservationsByTaxon(int taxonID, int currentpage)
    {
        return _db.GetObservationsByTaxon(taxonID,PageSize,currentpage);
    }

    public int GetObservationCountByTaxon(int taxonID)
    {
        return _db.GetObservationCountByTaxon(taxonID);
    }
}
