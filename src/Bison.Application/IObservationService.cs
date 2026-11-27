using Bison.Domain.Models;

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
    void Like (string userId, int postId, bool isLike);

    public List<Taxon> GetTaxons();

    public int GetObservationCountByTaxon(int taxonID);

    public List<ObservationViewModel> GetObservationsByTaxon(int taxonID, int currentpage);
}