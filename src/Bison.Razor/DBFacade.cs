using Microsoft.Data.Sqlite;

public class DBFacade
{
    private readonly string _connectionString;
    
    public DBFacade(string path)
{
    _connectionString = $"Data Source={path}";
}

    public List<ObservationViewModel> GetObservations(int pageSize, int page = 1) //page = 1 means its a default value so it shows page 1 first
    {
        var sql = """
            SELECT 
                o.observation_id,
                u.username, 
                o.text, 
                o.pub_date
                FROM observation o
            JOIN user u 
                ON o.author_id = u.user_id
            ORDER BY 
                o.pub_date DESC 
            LIMIT @limit OFFSET @offset
            """;
        return Query(sql, ("@limit", pageSize), ("@offset", (page - 1) * pageSize));
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int pageSize, int page = 1)
    {
        var sql = """
            SELECT 
                o.observation_id,
                u.username, 
                o.text, 
                o.pub_date
            FROM observation o
            JOIN user u 
                ON o.author_id = u.user_id
            WHERE u.username = @author
            ORDER BY 
                o.pub_date DESC
            LIMIT @limit OFFSET @offset
            """;
        return Query(sql, ("@limit", pageSize), ("@offset", (page - 1) * pageSize), ("@author", author));
    }

    public List<CommentViewModel> GetCommentsForObservation(int observationId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                u.username,
                c.text,
                c.pub_date
            FROM comment c
            JOIN user u ON u.user_id = c.author_id
            WHERE c.observation_id = @id
            ORDER BY c.pub_date
            """;

        command.Parameters.AddWithValue("@id", observationId);

        var comments = new List<CommentViewModel>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            comments.Add(new CommentViewModel(
                reader.GetString(0),
                reader.GetString(1),
                UnixTimeStampToDateTimeString(reader.GetInt64(2))
            ));
        }

        return comments;
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
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                UnixTimeStampToDateTimeString(reader.GetInt64(3))));
        }
        return result;
    }

    public List<ProposalViewModel> GetProposalsForObservation(int observationId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                u.username,
                p.text,
                p.pub_date
            FROM proposal p
            JOIN user u ON u.user_id = p.author_id
            WHERE p.observation_id = @id
            ORDER BY p.pub_date
            """;

        command.Parameters.AddWithValue("@id", observationId);

        var proposals = new List<ProposalViewModel>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            proposals.Add(new ProposalViewModel(
                reader.GetString(0),
                reader.GetString(1),
                UnixTimeStampToDateTimeString(reader.GetInt64(2))
            ));
        }
        return proposals;
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

    public ObservationViewModel? GetObservationById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT o.observation_id, u.username, o.text, o.pub_date
            FROM observation o
            JOIN user u ON u.user_id = o.author_id
            WHERE o.observation_id = @id
            """;

        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new ObservationViewModel(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            UnixTimeStampToDateTimeString(reader.GetInt64(3))
        );
    }
}
