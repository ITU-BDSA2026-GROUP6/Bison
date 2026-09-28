using Microsoft.EntityFrameworkCore;
public record ObservationViewModel(string Author, string Message, string Timestamp);

public interface IObservationService
{
    public List<ObservationViewModel> GetObservations();
    public List<ObservationViewModel> GetObservationsFromAuthor(string author);
}

public class ObservationService : IObservationService
{
    private readonly DBFacade _db;
    public ObservationService(DBFacade db)
    {
        _db = db;
    }

    public List<ObservationViewModel> GetObservations()
    {
        return _db.GetObservations();
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author)
    {
        // filter by the provided author name
        return _db.GetObservationsFromAuthor(author);
    }

    public void AddObservation(string author, string message)
    {
        _db.AddObservation(author, message);
    }
}
