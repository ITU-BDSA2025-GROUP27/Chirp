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
        using StreamReader sr = new(_filePath);
        using CsvReader cr = new(sr, CultureInfo.InvariantCulture);

        return cr.GetRecords<T>().ToList();
    }

    public void Store(T record)
    {
        using StreamWriter sw = File.AppendText(_filePath);
        using CsvWriter cw = new(sw, CultureInfo.InvariantCulture);

        cw.WriteRecord(record);
        cw.NextRecord();
    }
}