using CsvHelper;
using System.Globalization;

namespace SimpleDB;

public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private static CSVDatabase<T>? instance = null;
    private readonly string _filePath;

    private CSVDatabase(string filePath)
    {
        _filePath = filePath;
    }

    public static CSVDatabase<T> Instance(string filePath)
    {
        if (instance == null)
        {
            instance = new CSVDatabase<T>(filePath);
        }
        return instance;
    }

    public IEnumerable<T> Read(int? limit = null)
    {
        using StreamReader sr = new(_filePath);
        using CsvReader cr = new(sr, CultureInfo.InvariantCulture);

        var records = cr.GetRecords<T>();

        if (limit.HasValue)
        {
            return records.Take(limit.Value).ToList();
        }

        return records.ToList();
    }

    public void Store(T record)
    {
        bool hasHeader = File.Exists(_filePath) && new FileInfo(_filePath).Length > 0;

        using StreamWriter sw = File.AppendText(_filePath);
        using CsvWriter cw = new(sw, CultureInfo.InvariantCulture);

        // Write CSV header if file is newly created or empty
        if (!hasHeader)
        {
            cw.WriteHeader<T>();
            cw.NextRecord();
        }

        cw.WriteRecord(record);
        cw.NextRecord();
    }
}
