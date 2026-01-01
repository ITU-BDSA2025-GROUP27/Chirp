namespace Chirp.Core;

/// <summary>
/// Data access for authors and their follow relationships.
/// </summary>
public interface IAuthorRepository
{
    // Queries

    /// <summary>
    /// Finds an author by their username.
    /// </summary>
    Task<AuthorDTO?> FindAuthorByName(string name);

    /// <summary>
    /// Finds an author by their email address.
    /// </summary>
    Task<AuthorDTO?> FindAuthorByEmail(string email);

    /// <summary>
    /// Checks if one author is following another.
    /// </summary>
    Task<bool> IsFollowing(string followerName, string followedName);

    /// <summary>
    /// Gets the list of authors that a specific author is following.
    /// </summary>
    Task<List<AuthorDTO>> GetFollowing(string authorName);

    // Commands

    /// <summary>
    /// Creates a new author.
    /// </summary>
    Task CreateAuthor(Author author);

    /// <summary>
    /// Makes one author follow another.
    /// </summary>
    Task FollowAuthor(string followerName, string followedName);

    /// <summary>
    /// Makes one author unfollow another.
    /// </summary>
    Task UnfollowAuthor(string followerName, string followedName);
}
