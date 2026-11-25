namespace Bison.Domain.Models;

public abstract class Post
{
    public int PostId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public Author Author { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; }
}
