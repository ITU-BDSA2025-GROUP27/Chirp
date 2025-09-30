using SimpleDB;
using DocoptNet;

namespace Chirp.CLI;

class Program
{
    public static string file = "data/chirp_cli_db.csv";
    public static CSVDatabase<Cheep> db = new CSVDatabase<Cheep>(file);

    static void Main(string[] args)
    {
        var arguments = new Docopt().Apply(UserInterface.usage, args, version: "1.0", exit: true)!;

        if (arguments["read"].IsTrue)
        {
            ReadCheeps();
        }
        else if (arguments["cheep"].IsTrue)
        {
            string message = arguments["<message>"].ToString();
            WriteCheep(message);
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