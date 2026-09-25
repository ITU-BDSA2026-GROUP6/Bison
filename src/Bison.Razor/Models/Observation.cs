namespace Bison.Razor.Models;

public class Observation
{
    public int ObservationId { get; set; }
    public int UserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public User User { get; set; } = null!;

}