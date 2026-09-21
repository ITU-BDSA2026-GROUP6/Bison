using CsvHelper.Configuration.Attributes;

namespace SimpleDB;

public class Cheep
{
    public string Author { get; }
    public string Text { get; }
    public long Timestamp { get; }

    public Cheep(string author, string text, long timestamp)
    {
        Author = author;
        Text = text;
        Timestamp = timestamp;
    }
}
public class Observation : Cheep
{
    public long ObsID { get; }
    public string Location { get; }

    public Observation(
        long obsID,
        string author,
        string text,
        string location,
        long timestamp)
        : base(author, text, timestamp)
    {
        ObsID = obsID;
        Location = location;
    }
}
public class Comment : Cheep
{
    public long ObsID { get; }

    public Comment(
        long obsID,
        string author,
        string text,
        long timestamp)
        : base(author, text, timestamp)
    {
        ObsID = obsID;
    }
}

public class Proposal : Cheep
{
    public long ObsID { get; }

    [Ignore]
    public string TaxonID => Text;

    public Proposal(
        long obsID,
        string author,
        string text,
        long timestamp)
        : base(author, text, timestamp)
    {
        ObsID = obsID;
    }

}