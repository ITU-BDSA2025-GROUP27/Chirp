namespace SimpleDB.Tests;

public record TestRecord(string Author, string Message, long Timestamp);

public class CSVDatabaseTests : IDisposable
{
    private readonly string _testFilePath;

    public CSVDatabaseTests()
    {
        _testFilePath = Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public void Store_SingleRecord_RecordIsRetrieved()
    {
        // Arrange
        var db = new CSVDatabase<TestRecord>(_testFilePath);
        var record = new TestRecord("Lukas", "Hello", 1759590488);

        // Act
        db.Store(record);
        var results = db.Read().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("Lukas", results[0].Author);
        Assert.Equal("Hello", results[0].Message);
        Assert.Equal(1759590488, results[0].Timestamp);
    }

    [Fact]
    public void Store_MultipleRecords_AllAreRetrieved()
    {
        // Arrange
        var db = new CSVDatabase<TestRecord>(_testFilePath);
        var record1 = new TestRecord("Lukas", "Hello", 1759590523);
        var record2 = new TestRecord("John", "World", 1759590534);

        // Act
        db.Store(record1);
        db.Store(record2);
        var results = db.Read().ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Lukas", results[0].Author);
        Assert.Equal("John", results[1].Author);
    }
}
