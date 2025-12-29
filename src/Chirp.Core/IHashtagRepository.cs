namespace Chirp.Core;

public interface IHashtagRepository
{
    // Queries
    Task<Hashtag?> FindHashtagByName(string tagName);
    Task<List<HashtagDTO>> GetHashtagsForCheep(int cheepId);
    Task<List<string>> GetHashtagNamesInText(string text);

    // Commands
    Task<Hashtag> CreateHashtag(string tagName);
    Task LinkCheepToHashtag(int cheepId, int hashtagId);
}
