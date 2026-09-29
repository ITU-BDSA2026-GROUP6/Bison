using Microsoft.EntityFrameworkCore;
public record ObservationViewModel(string Author, string Message, string Timestamp);
public interface IObservationService
{
    public List<ObservationViewModel> GetObservations(int page);
    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int page);
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
    public void AddObservation(string author, string message)
    {
        _db.AddObservation(author, message);
    }
}
