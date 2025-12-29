namespace Chirp.Core;

public interface ICheepRepository
{
    // Queries
    Task<List<CheepDTO>> GetCheeps(int page);
    Task<List<CheepDTO>> GetCheepsByAuthor(string author, int page);
    Task<List<CheepDTO>> GetCheepsByAuthors(List<string> authors, int page);
    Task<List<CheepDTO>> GetCheepsByHashtag(string tagName, int page);

    // Commands
    Task CreateCheep(string authorName, string authorEmail, string text);
}
