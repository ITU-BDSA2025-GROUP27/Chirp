using Chirp.Core;

namespace Chirp.Infrastructure;

public class CheepService : ICheepService
{
    private readonly ICheepRepository _cheepRepository;

    public CheepService(ICheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }

    public List<CheepDTO> GetCheeps(int page = 1)
    {
        return _cheepRepository.GetCheeps(page).Result;
    }

    public List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1)
    {
        return _cheepRepository.GetCheepsByAuthor(author, page).Result;
    }
}
