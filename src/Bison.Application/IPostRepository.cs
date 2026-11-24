using Bison.Domain.Models;


public record PersonalData
(
    string UserId,
    string? Username,
    string Email,
    IReadOnlyList<Observation> Posts,
    IReadOnlyList<Comment> Comments,
    IReadOnlyList<Proposal> Proposals
);
public interface IPostRepository
{
    List<Observation> GetObservations(int pageSize, int page);
    List<Observation> GetObservationsByAuthor(string authorName, int pageSize, int page);
    List<Observation> GetObservationsByTaxon(int taxonId, int pageSize, int page);

    PersonalData? GetPersonalData(string userId);
    int GetObservationCount();
    int GetObservationCountByAuthor(string authorName);
    int GetObservationCountByTaxon(int taxonId);

    Observation? GetObservationById(int id); // inkl. Comments og Proposals

    List<Comment> GetCommentsForObservation(int observationId);
    List<Proposal> GetProposalsForObservation(int observationId);

    void AddObservation(string userId, string text, int? taxonId = null, Picture? picture = null);

    void AddComment(string userId, int observationId, string text);
    void AddProposal(string userId, int observationId, int taxonId, string text);

    List<Taxon> GetTaxons();
}
