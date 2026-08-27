using System;
using System.IO;

public class Observation
{
    public string Author { get; set; } = string.Empty;
    public string ObservationText { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}

class Program
{
    static void Main(string [] args)
    {
        
        try
        {
           if (args[0] == "observe")
            {
                Observe(string.Join(" ", args[1..]));
            }
            else
            {
                Console.WriteLine("Invalid command. Use 'observe' followed by observation text.");
            } 
            
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

    static void Observe(string message)
    {
           Observation observation = new Observation
            {
                ObservationText = message,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                Author = Environment.UserName
            };

            string csvLine = $"{observation.Author},{observation.ObservationText},{observation.Timestamp}";
            string filePath = Path.Combine("CSVfiles", "bison_observe_cli_db.csv");

            try
            {
                using StreamWriter writer = new(filePath, append: true);
                writer.WriteLine(csvLine);
                Console.WriteLine("Observation saved successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't write to file: {e.Message}");
            }
        
    }
    
}
