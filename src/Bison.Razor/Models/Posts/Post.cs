namespace Bison.Razor.Models;

public abstract class Post
{
    public int PostId { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; }
}