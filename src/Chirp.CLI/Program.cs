using DocoptNet;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Chirp.CLI;

public class Program
{
    private const string baseURL = "http://localhost:5272";

    static async Task Main(string[] args)
    {
        var arguments = new Docopt().Apply(UserInterface.usage, args, version: "1.0", exit: true)!;

        if (arguments["read"].IsTrue)
        {
            int? limit = null;
            if (arguments["<limit>"].Value != null)
            {
                limit = int.Parse(arguments["<limit>"].ToString());
            }
            await ReadCheeps(limit);
        }
        else if (arguments["cheep"].IsTrue)
        {
            string message = arguments["<message>"].ToString();
            await WriteCheep(message);
        }
    }

    static async Task ReadCheeps(int? limit = null)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.BaseAddress = new Uri(baseURL);

        var cheeps = await client.GetFromJsonAsync<List<Cheep>>("cheeps");

        if (cheeps != null)
        {
            var cheepsToPrint = limit.HasValue ? cheeps.Take(limit.Value) : cheeps;
            foreach (var cheep in cheepsToPrint)
            {
                UserInterface.PrintCheep(cheep);
            }
        }
    }

    static async Task WriteCheep(string message)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.BaseAddress = new Uri(baseURL);

        string author = GetUserName();
        long timestamp = FromCurrentDateTimetoUnixTime();

        Cheep cheep = new(author, message, timestamp);

        await client.PostAsJsonAsync("cheep", cheep);
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