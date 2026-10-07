using Bison.Razor.Models;

public interface IPostRepository
{
    List<Observation> GetObservations(int pageSize, int page);
    List<Observation> GetObservationsByAuthor(string authorName, int pageSize, int page);
    List<Observation> GetObservationsByTaxon(int taxonId, int pageSize, int page);

    int GetObservationCount();
    int GetObservationCountByAuthor(string authorName);
    int GetObservationCountByTaxon(int taxonId);

    Observation? GetObservationById(int id); // inkl. Comments og Proposals

    List<Comment> GetCommentsForObservation(int observationId);
    List<Proposal> GetProposalsForObservation(int observationId);

    void AddObservation(string authorName, string text);
}