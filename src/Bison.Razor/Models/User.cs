namespace Bison.Razor.Models;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public long Timestamp { get; set; }

}