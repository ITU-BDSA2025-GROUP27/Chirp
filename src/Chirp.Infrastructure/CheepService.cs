using Chirp.Core;

namespace Chirp.Infrastructure;

public class CheepService : ICheepService
{
    private readonly ICheepRepository _cheepRepository;
    private readonly IAuthorRepository _authorRepository;

    public CheepService(ICheepRepository cheepRepository, IAuthorRepository authorRepository)
    {
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
    }

    public List<CheepDTO> GetCheeps(int page = 1)
    {
        return _cheepRepository.GetCheeps(page).Result;
    }

    public List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1)
    {
        return _cheepRepository.GetCheepsByAuthor(author, page).Result;
    }

    public List<CheepDTO> GetCheepsFromAuthors(List<string> authors, int page = 1)
    {
        return _cheepRepository.GetCheepsByAuthors(authors, page).Result;
    }

    public async Task CreateCheep(string authorName, string authorEmail, string text)
    {
        await _cheepRepository.CreateCheep(authorName, authorEmail, text);
    }

    public async Task<bool> IsFollowing(string followerName, string followedName)
    {
        return await _authorRepository.IsFollowing(followerName, followedName);
    }

    public async Task<List<string>> GetFollowing(string authorName)
    {
        return await _authorRepository.GetFollowing(authorName);
    }

    public async Task FollowAuthor(string followerName, string followedName)
    {
        await _authorRepository.FollowAuthor(followerName, followedName);
    }

    public async Task UnfollowAuthor(string followerName, string followedName)
    {
        await _authorRepository.UnfollowAuthor(followerName, followedName);
    }
}
