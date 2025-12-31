namespace Chirp.Core;

public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page = 1);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1);
    List<CheepDTO> GetCheepsFromAuthors(List<string> authors, int page = 1);
    List<CheepDTO> GetCheepsByHashtag(string tagName, int page = 1);
    Task CreateCheep(string authorName, string authorEmail, string text);
}
