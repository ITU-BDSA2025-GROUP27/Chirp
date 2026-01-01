namespace Chirp.Core;

/// <summary>
/// Business logic for author follow/unfollow functionality.
/// </summary>
public interface IAuthorService
{
    Task<bool> IsFollowing(string followerName, string followedName);
    Task<List<AuthorDTO>> GetFollowing(string authorName);
    Task FollowAuthor(string followerName, string followedName);
    Task UnfollowAuthor(string followerName, string followedName);
}
