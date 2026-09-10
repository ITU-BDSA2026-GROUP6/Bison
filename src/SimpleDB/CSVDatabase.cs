using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace SimpleDB;
public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private readonly string _filePath;
    private static readonly CsvConfiguration Config = new(CultureInfo.InvariantCulture)
    {
        PrepareHeaderForMatch = args => args.Header.ToLower(),
    };

    public CSVDatabase(string filePath)
    {
        _filePath = filePath;
    }

    public IEnumerable<T> Read(int? limit = null)
    {
        using StreamReader reader = new(_filePath);
        using CsvReader csv = new(reader, Config);

        List<T> records = csv.GetRecords<T>().ToList();

        if (limit.HasValue)
        {
            return records.Take(limit.Value).ToList();
        }

        return records.ToList();
    }

    public void Store(T record)
    {
        bool needsHeader = !File.Exists(_filePath) || new FileInfo(_filePath).Length == 0;

        using StreamWriter writer = new(_filePath, append: true);
        using var csv = new CsvWriter(writer, Config);

        if (needsHeader)
        {
            csv.WriteHeader<T>();
            csv.NextRecord();
        }

        csv.WriteRecord(record);
        csv.NextRecord();
    }
}