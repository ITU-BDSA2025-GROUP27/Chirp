using SimpleDB;

class Program
{
    public static string file = "data/chirp_cli_db.csv";
    public static string timeFormat = "MM/dd/yy HH:mm:ss";
    public static CSVDatabase<Cheep> db = new CSVDatabase<Cheep>(file);

    static void Main(string[] args)
    {

        // If no args, print usage message
        if (args.Length == 0)
        {
            Console.WriteLine("-------------");
            Console.WriteLine("Usage:");
            Console.WriteLine("dotnet run -- read | Prints all Cheeps");
            Console.WriteLine("dotnet run -- cheep <message> | Cheep a message");
            Console.WriteLine("-------------");
        }
        else if (args[0] == "read")
        {
            ReadCheeps();
        }
        else if (args[0] == "cheep")
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Write a message to Cheep!");
            }
            WriteCheep(args[1]);
        }
    }

    static void ReadCheeps()
    {
        foreach (var r in db.Read())
        {
            Console.WriteLine($"{r.Author} @ {FromUnixTimeToDateTime(r.Timestamp)} : {r.Message}");
        }
    }

    static void WriteCheep(string message)
    {
        string author = GetUserName();
        long timestamp = FromCurrentDateTimetoUnixTime();

        Cheep cheep = new(author, message, timestamp);

        db.Store(cheep);
    }

    static string GetUserName()
    {
        return Environment.UserName;
    }

    static string FromUnixTimeToDateTime(long timestamp)
    {
        var dto = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime();
        return dto.ToString(timeFormat);
    }

    static long FromCurrentDateTimetoUnixTime()
    {
        return ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
    }

    public record Cheep(string Author, string Message, long Timestamp);
}