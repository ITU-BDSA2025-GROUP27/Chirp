namespace Chirp.CLI;

public static class UserInterface
{
    public static string timeFormat = "MM/dd/yy HH:mm:ss";

    public static void PrintUsage()
    {
        Console.WriteLine("-------------");
        Console.WriteLine("Usage:");
        Console.WriteLine("dotnet run -- read | Prints all Cheeps");
        Console.WriteLine("dotnet run -- cheep <message> | Cheep a message");
        Console.WriteLine("-------------");
    }

    public static void PrintMissingMessageError()
    {
        Console.WriteLine("Write a message to Cheep!");
    }

    public static void PrintCheep(Cheep cheep)
    {
        Console.WriteLine($"{cheep.Author} @ {FromUnixTimeToDateTime(cheep.Timestamp)} : {cheep.Message}");
    }

    static string FromUnixTimeToDateTime(long timestamp)
    {
        var dto = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime();
        return dto.ToString(timeFormat);
    }
}