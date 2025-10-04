namespace Chirp.CLI.Tests;

public class UnitTest1
{
    [Fact]
    public void FromUnixTimeToDateTime_ConvertsCorrectly()
    {
        // Arrange
        long unixTime = 1759589890; // 04/10/25 16:58:45 UTC 
        DateTimeOffset expectedDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime();
        string expectedString = expectedDateTime.ToString("MM/dd/yy HH:mm:ss");

        // Act
        string result = UserInterface.FromUnixTimeToDateTime(unixTime);

        // Assert
        Assert.Equal(expectedString, result);
    }
}
