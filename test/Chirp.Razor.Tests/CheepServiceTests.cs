namespace Chirp.Razor.Tests;

public class CheepServiceTests
{
    [Fact]
    public void UnixTimeStampToDateTime_ConvertsCorrectly()
    {
        // Arrange
        long unixTime = 1690892208; // Used in test data for Helge's cheep
        DateTimeOffset expectedDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        string expectedString = expectedDateTime.ToString("MM/dd/yy H:mm:ss");

        // Act
        string result = ConvertUnixTimeStamp(unixTime);

        // Assert
        Assert.Equal(expectedString, result);
    }

    // Helper method that mimics CheepService.UnixTimeStampToDateTimeString
    private static string ConvertUnixTimeStamp(double unixTimeStamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }
}
