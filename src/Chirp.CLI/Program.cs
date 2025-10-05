using SimpleDB;
using DocoptNet;

namespace Chirp.CLI;

public class Program
{
    public static string file = "../../data/chirp_cli_db.csv";
    public static CSVDatabase<Cheep> db = CSVDatabase<Cheep>.Instance(file);

    static void Main(string[] args)
    {
        var arguments = new Docopt().Apply(UserInterface.usage, args, version: "1.0", exit: true)!;

        if (arguments["read"].IsTrue)
        {
            int? limit = null;
            if (arguments["<limit>"].Value != null)
            {
                limit = int.Parse(arguments["<limit>"].ToString());
            }
            ReadCheeps(limit);
        }
        else if (arguments["cheep"].IsTrue)
        {
            string message = arguments["<message>"].ToString();
            WriteCheep(message);
        }
    }

    static void ReadCheeps(int? limit = null)
    {
        foreach (var r in db.Read(limit))
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

    public static long FromCurrentDateTimetoUnixTime()
    {
        return ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
    }
}

public record Cheep(string Author, string Message, long Timestamp);