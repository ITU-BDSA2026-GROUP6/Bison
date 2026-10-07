namespace Bison.Razor.Models;

public class Author
{
    public int AuthorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}