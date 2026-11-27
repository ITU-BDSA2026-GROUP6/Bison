namespace Bison.Domain.Models;

public class Like
{
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public string AuthorId { get; set; } = string.Empty;
    public Author Author { get; set; } = null!;

    public bool IsLike { get; set; }
}