namespace Chirp.Core;

/// <summary>
/// Data access for authors and their follow relationships.
/// </summary>
public interface IAuthorRepository
{
    // Queries
    Task<AuthorDTO?> FindAuthorByName(string name);
    Task<AuthorDTO?> FindAuthorByEmail(string email);
    Task<bool> IsFollowing(string followerName, string followedName);
    Task<List<AuthorDTO>> GetFollowing(string authorName);

    // Commands
    Task CreateAuthor(Author author);
    Task FollowAuthor(string followerName, string followedName);
    Task UnfollowAuthor(string followerName, string followedName);
}
