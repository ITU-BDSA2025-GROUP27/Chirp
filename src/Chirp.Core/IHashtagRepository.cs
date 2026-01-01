namespace Chirp.Core;

/// <summary>
/// Data access for hashtags and their links to cheeps.
/// </summary>
public interface IHashtagRepository
{
    // Queries

    /// <summary>
    /// Finds a hashtag by its name.
    /// </summary>
    Task<Hashtag?> FindHashtagByName(string tagName);

    /// <summary>
    /// Gets all hashtags associated with a specific cheep.
    /// </summary>
    Task<List<HashtagDTO>> GetHashtagsForCheep(int cheepId);

    /// <summary>
    /// Parses text and extracts hashtag names.
    /// </summary>
    Task<List<string>> GetHashtagNamesInText(string text);

    // Commands

    /// <summary>
    /// Creates a new hashtag.
    /// </summary>
    Task<Hashtag> CreateHashtag(string tagName);

    /// <summary>
    /// Links a cheep to a hashtag.
    /// </summary>
    Task LinkCheepToHashtag(int cheepId, int hashtagId);
}
