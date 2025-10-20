namespace Chirp.Razor;

public interface ICheepRepository
{
    Task<List<Cheep>> GetCheeps(int page);
    Task<List<Cheep>> GetCheepsByAuthor(string author, int page);
}
