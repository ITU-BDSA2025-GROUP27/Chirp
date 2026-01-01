namespace Chirp.Core;

/// <summary>
/// Business logic for author follow/unfollow functionality.
/// </summary>
public interface IAuthorService
{
    /// <summary>
    /// Checks if one author is following another.
    /// </summary>
    Task<bool> IsFollowing(string followerName, string followedName);

    /// <summary>
    /// Gets the list of authors that a specific author is following.
    /// </summary>
    Task<List<AuthorDTO>> GetFollowing(string authorName);

    /// <summary>
    /// Makes one author follow another.
    /// </summary>
    Task FollowAuthor(string followerName, string followedName);

    /// <summary>
    /// Makes one author unfollow another.
    /// </summary>
    Task UnfollowAuthor(string followerName, string followedName);
}
