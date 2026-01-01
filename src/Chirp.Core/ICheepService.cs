namespace Chirp.Core;

/// <summary>
/// Business logic for cheeps - coordinates between repositories and handles hashtag extraction.
/// </summary>
public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page = 1);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1);
    List<CheepDTO> GetCheepsFromAuthors(List<string> authors, int page = 1);
    List<CheepDTO> GetCheepsByHashtag(string tagName, int page = 1);

    /// <summary>
    /// Creates a cheep and extracts/links hashtags. Also creates the author if they don't exist yet.
    /// </summary>
    Task CreateCheep(string authorName, string authorEmail, string text);
}
