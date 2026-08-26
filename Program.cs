public class Observation
{
    public string Author { get; set; } = string.Empty;
    public string ObservationText { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}

class Program
{
    static void Main()
    {
        try
        {
            string filePath = Path.Combine("CSVfiles", "bison_observe_cli_db.csv");
            using StreamReader reader = new(filePath);
            string text = reader.ReadToEnd();

            Console.WriteLine(text);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't read file: {e.Message}"); 
        }
    }
    
}
