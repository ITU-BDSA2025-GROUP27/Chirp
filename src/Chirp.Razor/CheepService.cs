namespace Chirp.Razor;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps(int page = 1);
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 1);
}

public class CheepService : ICheepService
{
    private readonly ICheepRepository _cheepRepository;

    public CheepService(ICheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }

    public List<CheepViewModel> GetCheeps(int page = 1)
    {
        var cheepsFromDb = _cheepRepository.GetCheeps(page).Result;
        return cheepsFromDb
            .Select(c => new CheepViewModel(
                c.Author.Name,
                c.Text,
                c.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
            .ToList();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 1)
    {
        var cheepsFromDb = _cheepRepository.GetCheepsByAuthor(author, page).Result;
        return cheepsFromDb
            .Select(c => new CheepViewModel(
                c.Author.Name,
                c.Text,
                c.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
            .ToList();
    }
}
