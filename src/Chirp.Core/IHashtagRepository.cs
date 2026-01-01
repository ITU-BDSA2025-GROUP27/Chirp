namespace Chirp.Core;

/// <summary>
/// Data access for hashtags and their links to cheeps.
/// </summary>
public interface IHashtagRepository
{
    // Queries
    Task<Hashtag?> FindHashtagByName(string tagName);
    Task<List<HashtagDTO>> GetHashtagsForCheep(int cheepId);

    /// <summary>
    /// Parses text and extracts hashtag names.
    /// </summary>
    Task<List<string>> GetHashtagNamesInText(string text);

    // Commands
    Task<Hashtag> CreateHashtag(string tagName);
    Task LinkCheepToHashtag(int cheepId, int hashtagId);
}
