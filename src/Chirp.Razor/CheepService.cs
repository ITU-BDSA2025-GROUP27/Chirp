namespace Chirp.Razor;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps(int page = 1);
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 1);
}

public class CheepService : ICheepService
{
    private readonly DBFacade _dbFacade;

    public CheepService(DBFacade dbFacade)
    {
        _dbFacade = dbFacade;
    }

    public List<CheepViewModel> GetCheeps(int page = 1)
    {
        const int limit = 32;
        var offset = (page - 1) * limit;

        var cheepsFromDb = _dbFacade.GetAllCheeps(limit, offset);
        return cheepsFromDb
            .Select(c => new CheepViewModel(
                c.Author,
                c.Message,
                UnixTimeStampToDateTimeString(c.Timestamp)))
            .ToList();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 1)
    {
        const int limit = 32;
        var offset = (page - 1) * limit;

        var cheepsFromDb = _dbFacade.GetCheepsByAuthor(author, limit, offset);
        return cheepsFromDb
            .Select(c => new CheepViewModel(
                c.Author,
                c.Message,
                UnixTimeStampToDateTimeString(c.Timestamp)))
            .ToList();
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }
}
