namespace Chirp.Core;

public interface IAuthorRepository
{
    // Queries
    Task<Author?> FindAuthorByName(string name);
    Task<Author?> FindAuthorByEmail(string email);
    Task<bool> IsFollowing(string followerName, string followedName);
    Task<List<string>> GetFollowing(string authorName);

    // Commands
    Task CreateAuthor(Author author);
    Task FollowAuthor(string followerName, string followedName);
    Task UnfollowAuthor(string followerName, string followedName);
}
