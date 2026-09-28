using Microsoft.Data.Sqlite;

public class DBFacade
{
    private readonly string _connectionString;

    public DBFacade()
    {
        var path = Environment.GetEnvironmentVariable("BISONDBPATH")
                   ?? Path.Combine(Path.GetTempPath(), "bison.db");
        _connectionString = $"Data Source={path}";
    }

    public List<ObservationViewModel> GetObservations()
    {
        var sql = """
            SELECT u.username, o.text, o.pub_date
            FROM observation o
            JOIN user u ON o.author_id = u.user_id
            ORDER BY o.pub_date DESC
            """;
        return Query(sql);
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author)
    {
        var sql = """
            SELECT u.username, o.text, o.pub_date
            FROM observation o
            JOIN user u ON o.author_id = u.user_id
            WHERE u.username = @author
            ORDER BY o.pub_date DESC
            """;
        return Query(sql, ("@author", author));
    }

    private List<ObservationViewModel> Query(string sql, params (string Name, object Value)[] parameters)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        var result = new List<ObservationViewModel>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ObservationViewModel(
                reader.GetString(0),
                reader.GetString(1),
                UnixTimeStampToDateTimeString(reader.GetInt64(2))));
        }
        return result;
    }

    public void AddObservation(String author, string text)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
        INSERT INTO observation (author_id, text, pub_date)
        SELECT user_id, @text, @pubDate
        FROM user
        WHERE username = @author
        """;

        command.Parameters.AddWithValue("@author", author);
        command.Parameters.AddWithValue("@text", text);
        command.Parameters.AddWithValue("@pubDate", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        command.ExecuteNonQuery();
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }
}