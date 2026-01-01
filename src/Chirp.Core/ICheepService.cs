namespace Chirp.Core;

/// <summary>
/// Business logic for cheeps - coordinates between repositories and handles hashtag extraction.
/// </summary>
public interface ICheepService
{
    /// <summary>
    /// Gets all cheeps across the application.
    /// </summary>
    List<CheepDTO> GetCheeps(int page = 1);

    /// <summary>
    /// Gets cheeps from a specific author.
    /// </summary>
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1);

    /// <summary>
    /// Gets cheeps from multiple authors - used for displaying a user's timeline with their followed authors.
    /// </summary>
    List<CheepDTO> GetCheepsFromAuthors(List<string> authors, int page = 1);

    /// <summary>
    /// Gets cheeps containing a specific hashtag.
    /// </summary>
    List<CheepDTO> GetCheepsByHashtag(string tagName, int page = 1);

    /// <summary>
    /// Creates a cheep and extracts/links hashtags. Also creates the author if they don't exist yet.
    /// </summary>
    Task CreateCheep(string authorName, string authorEmail, string text);
}
