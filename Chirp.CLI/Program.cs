using SimpleDB;

namespace Chirp.CLI;

class Program
{
    public static string file = "data/chirp_cli_db.csv";
    public static CSVDatabase<Cheep> db = new CSVDatabase<Cheep>(file);

    static void Main(string[] args)
    {

        // If no args, print usage message
        if (args.Length == 0)
        {
            UserInterface.PrintUsage();
        }
        else if (args[0] == "read")
        {
            ReadCheeps();
        }
        else if (args[0] == "cheep")
        {
            if (args.Length < 2)
            {
                UserInterface.PrintMissingMessageError();
                return;
            }
            WriteCheep(args[1]);
        }
    }

    static void ReadCheeps()
    {
        foreach (var r in db.Read())
        {
            UserInterface.PrintCheep(r);
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

    static long FromCurrentDateTimetoUnixTime()
    {
        return ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
    }
}

public record Cheep(string Author, string Message, long Timestamp);