using Microsoft.EntityFrameworkCore;
public record ObservationViewModel(string Author, string Message, string Timestamp);

public interface IObservationService
{
    public Task<List<ObservationViewModel>> GetObservations();
    public List<ObservationViewModel> GetObservationsFromAuthor(string author);
}

public class ObservationService : IObservationService
{
    private readonly BisonDbContext _context;
    public ObservationService(BisonDbContext context)
    {
        _context = context;
    }
    private static readonly List<ObservationViewModel> _obs = new()
    {

    };

    public async Task<List<ObservationViewModel>> GetObservations()
    {
        // Define the query - with our setup, EF Core translates this to an SQLite query in the background
        var query = from message in _context.Messages
                    where message.User.Name == "Peter"
                    select new { message.Text, message.User, message.User.Email, message.User.Timestamp };

        var result = await query.ToListAsync();

        return result.Select(r => new ObservationViewModel(
        r.User.Name,
        r.Text,
        UnixTimeStampToDateTimeString(r.User.Timestamp)
    )).ToList();
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author)
    {
        // filter by the provided author name
        return _obs.Where(x => x.Author == author).ToList();
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

}
