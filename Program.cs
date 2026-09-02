using System;
using System.IO;
using CsvHelper;
using System.Globalization;

public record Cheep(string Author, string Message, long Timestamp);

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
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                string author = csv.GetField<string>("Author")!;
                string message = csv.GetField<string>("Observation")!;
                long timestamp = csv.GetField<long>("Timestamp");

                Cheep cheep = new Cheep(author, message, timestamp);
                Console.WriteLine($"Author: {cheep.Author}, Observation: {cheep.Message}, Timestamp: {cheep.Timestamp}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't read file: {e.Message}");
        }
    }

    static void Observe(string message)
    {
        Cheep cheep = new Cheep(Environment.UserName, message, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        string filePath = Path.Combine("CSVfiles", "bison_observe_cli_db.csv");

        try
        {
            using StreamWriter writer = new(filePath, append: true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecord(cheep);
            csv.NextRecord();
            Console.WriteLine("Observation saved successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't write to file: {e.Message}");
        }
    }
}