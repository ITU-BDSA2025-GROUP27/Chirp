namespace Chirp.Core;

/// <summary>
/// Data access for cheeps. All queries return results paginated (32 per page) and sorted newest first.
/// </summary>
public interface ICheepRepository
{
    // Queries

    /// <summary>
    /// Gets all cheeps across the application.
    /// </summary>
    Task<List<CheepDTO>> GetCheeps(int page);

    /// <summary>
    /// Gets cheeps from a specific author.
    /// </summary>
    Task<List<CheepDTO>> GetCheepsByAuthor(string author, int page);

    /// <summary>
    /// Gets cheeps from multiple authors - used for displaying a user's timeline with their followed authors.
    /// </summary>
    Task<List<CheepDTO>> GetCheepsByAuthors(List<string> authors, int page);

    /// <summary>
    /// Gets cheeps containing a specific hashtag.
    /// </summary>
    Task<List<CheepDTO>> GetCheepsByHashtag(string tagName, int page);

    // Commands

    /// <summary>
    /// Creates a new cheep.
    /// </summary>
    Task CreateCheep(string authorName, string text);
}
