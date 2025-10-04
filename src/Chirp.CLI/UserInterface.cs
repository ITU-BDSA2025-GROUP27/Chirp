using DocoptNet;

namespace Chirp.CLI;

public static class UserInterface
{
    public static string timeFormat = "MM/dd/yy HH:mm:ss";

    public const string usage = @"Chirp CLI

Usage:
  chirp read
  chirp cheep <message>
  chirp (-h | --help)
  chirp --version

Options:
  -h --help     Show this screen.
  --version     Show version.
";

    public static void PrintUsage()
    {
        Console.WriteLine(usage);
    }

    public static void PrintMissingMessageError()
    {
        Console.WriteLine("Write a message to Cheep!");
    }

    public static void PrintCheep(Cheep cheep)
    {
        Console.WriteLine($"{cheep.Author} @ {FromUnixTimeToDateTime(cheep.Timestamp)} : {cheep.Message}");
    }

    public static string FromUnixTimeToDateTime(long timestamp)
    {
        var dto = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime();
        return dto.ToString(timeFormat);
    }
}