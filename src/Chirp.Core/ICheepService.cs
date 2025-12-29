namespace Chirp.Core;

public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page = 1);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1);
    List<CheepDTO> GetCheepsFromAuthors(List<string> authors, int page = 1);
    List<CheepDTO> GetCheepsByHashtag(string tagName, int page = 1);
    Task CreateCheep(string authorName, string authorEmail, string text);
    Task<bool> IsFollowing(string followerName, string followedName);
    Task<List<AuthorDTO>> GetFollowing(string authorName);
    Task FollowAuthor(string followerName, string followedName);
    Task UnfollowAuthor(string followerName, string followedName);
}
