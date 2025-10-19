namespace Chirp.Razor;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps();
    public List<CheepViewModel> GetCheepsFromAuthor(string author);
}

public class CheepService : ICheepService
{
    private readonly DBFacade _dbFacade;

    public CheepService(DBFacade dbFacade)
    {
        _dbFacade = dbFacade;
    }

    public List<CheepViewModel> GetCheeps()
    {
        var cheepsFromDb = _dbFacade.GetAllCheeps();
        return cheepsFromDb
            .Select(c => new CheepViewModel(
                c.Author,
                c.Message,
                UnixTimeStampToDateTimeString(c.Timestamp)))
            .ToList();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        var cheepsFromDb = _dbFacade.GetCheepsByAuthor(author);
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
