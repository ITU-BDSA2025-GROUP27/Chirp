namespace Chirp.Core;

public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page = 1);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1);
}
