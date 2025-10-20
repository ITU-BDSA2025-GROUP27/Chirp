namespace Chirp.Razor;

public interface ICheepRepository
{
    Task<List<CheepDTO>> GetCheeps(int page);
    Task<List<CheepDTO>> GetCheepsByAuthor(string author, int page);
}
