using Chirp.Core;

namespace Chirp.Infrastructure;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<bool> IsFollowing(string followerName, string followedName)
    {
        return await _authorRepository.IsFollowing(followerName, followedName);
    }

    public async Task<List<AuthorDTO>> GetFollowing(string authorName)
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
