using CsvHelper;
using System.Globalization;

namespace SimpleDB;
public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private readonly string _filePath;

    public CSVDatabase(string filePath)
    {
        _filePath = filePath;
    }

    public IEnumerable<T> Read(int? limit = null)
    {
        using StreamReader reader = new(_filePath);
        using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

        List<T> records = csv.GetRecords<T>().ToList();

        if (limit.HasValue)
        {
            return records.Take(limit.Value).ToList();
        }

        return records.ToList();
    }

    public void Store(T record)
    {
        using StreamWriter writer = new(_filePath, append: true);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecord(record);
        csv.NextRecord();
    }
}