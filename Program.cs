class Program
{
    public static string file = "data/chirp_cli_db.csv";
    public static string timeFormat = "MM/dd/yy HH:mm:ss";

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
            ReadCheepsFromCSV();
        }
        else if (args[0] == "cheep")
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Write a message to Cheep!");
            }
            WriteCheepToCSV(args[1]);
        }
    }

    static void ReadCheepsFromCSV()
    {
        using StreamReader sr = new(file);

        _ = sr.ReadLine(); // Skip header
        while (!sr.EndOfStream)
        {
            string? line = sr.ReadLine();
            string cheep = ParseCheepLine(line);

            Console.WriteLine(cheep);
        }
    }

    static void WriteCheepToCSV(string message)
    {
        string author = GetUserName();
        string timestamp = FromCurrentDateTimetoUnixTime();

        string cheep = $"{author},\"{message}\",{timestamp}";

        using StreamWriter sw = File.AppendText(file);
        sw.WriteLine(cheep);

    }

    /*
    * Utils
    */
    static string ParseCheepLine(string line)
    {
        string[] cheepData = line.Split('"'); // Split by " so commas inside the message aren't treated as separators

        string author = cheepData[0].TrimEnd(',');
        string message = cheepData[1];
        string timestamp = FromUnixTimeToDateTime(cheepData[2].TrimStart(','));

        return $"{author} @ {timestamp}: {message}";
    }

    static string GetUserName()
    {
        return Environment.UserName;
    }

    static string FromUnixTimeToDateTime(string timestamp)
    {
        var dto = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).ToLocalTime();
        return dto.ToString(timeFormat);
    }

    static string FromCurrentDateTimetoUnixTime()
    {
        return ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();
    }
}