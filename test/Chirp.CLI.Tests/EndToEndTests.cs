using System.Globalization;
using CsvHelper;

namespace Chirp.CLI.Tests;

public class EndToEndTests : IDisposable
{
    private readonly string _testFilePath;

    public EndToEndTests()
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
    public void Read_ProducesExpectedOutput()
    {
        // Arrange
        var cheeps = new List<Cheep>
        {
            new Cheep("ropf", "Hello, BDSA students!", 1690891760),
            new Cheep("adho", "Welcome to the course!", 1690978778),
            new Cheep("adho", "I hope you had a good summer.", 1690979858),
            new Cheep("ropf", "Cheeping cheeps on Chirp :)", 1690981487)
        };

        using (var writer = new StreamWriter(File.Open(_testFilePath, FileMode.Create)))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(cheeps);
        }

        var db = new SimpleDB.CSVDatabase<Cheep>(_testFilePath);

        // Act
        using var sw = new StringWriter();
        Console.SetOut(sw);

        foreach (var cheep in db.Read(10))
        {
            UserInterface.PrintCheep(cheep);
        }

        var output = sw.ToString();

        // Assert
        Assert.Contains("ropf", output);
        Assert.Contains("Hello, BDSA students!", output);
        Assert.Contains("adho", output);
        Assert.Contains("Welcome to the course!", output);
        Assert.Contains("I hope you had a good summer.", output);
        Assert.Contains("Cheeping cheeps on Chirp :)", output);
    }

    [Fact]
    public void Cheep_StoresMessageInDatabase()
    {
        // Arrange
        var db = new SimpleDB.CSVDatabase<Cheep>(_testFilePath);
        string testMessage = "Hello!!!";
        string author = Environment.UserName;

        // Act
        db.Store(new Cheep(author, testMessage, Program.FromCurrentDateTimetoUnixTime()));

        var results = db.Read().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(author, results[0].Author);
        Assert.Equal(testMessage, results[0].Message);
    }
}
