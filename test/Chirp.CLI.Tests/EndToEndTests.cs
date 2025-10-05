namespace Chirp.CLI.Tests;

// Note: Full end-to-end tests with HTTP are in Chirp.CSVDBService.Tests
// These tests verify CLI UI functionality
public class EndToEndTests
{
    [Fact]
    public void PrintCheep_FormatsOutputCorrectly()
    {
        // Arrange
        var cheep = new Cheep("ropf", "Hello, BDSA students!", 1690891760);

        // Act
        using var sw = new StringWriter();
        Console.SetOut(sw);
        UserInterface.PrintCheep(cheep);
        var output = sw.ToString();

        // Assert
        Assert.Contains("ropf", output);
        Assert.Contains("Hello, BDSA students!", output);
        Assert.Contains("08/01/23", output); // Date formatted
    }

    [Fact]
    public void FromCurrentDateTimetoUnixTime_ReturnsValidTimestamp()
    {
        // Act
        var timestamp = Program.FromCurrentDateTimetoUnixTime();

        // Assert - timestamp should be roughly current time (within last minute)
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
        Assert.InRange(timestamp, now - 60, now + 1);
    }
}
