using Chirp.Core;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Chirp.Infrastructure;

public class HashtagRepository : IHashtagRepository
{
    private readonly ChirpDBContext _dbContext;

    public HashtagRepository(ChirpDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Hashtag?> FindHashtagByName(string tagName)
    {
        var normalizedTag = tagName.ToLowerInvariant();
        return await _dbContext.Hashtags
            .FirstOrDefaultAsync(h => h.TagName == normalizedTag);
    }

    public async Task<List<HashtagDTO>> GetHashtagsForCheep(int cheepId)
    {
        var cheep = await _dbContext.Cheeps
            .Include(c => c.Hashtags)
            .FirstOrDefaultAsync(c => c.CheepId == cheepId);

        if (cheep == null)
        {
            return new List<HashtagDTO>();
        }

        return cheep.Hashtags.Select(h => new HashtagDTO(h.TagName)).ToList();
    }

    public async Task<List<string>> GetHashtagNamesInText(string text)
    {
        // Extract hashtags using regex: #[a-zA-Z0-9]+
        var regex = new Regex(@"#([a-zA-Z0-9]+)", RegexOptions.Compiled);
        var matches = regex.Matches(text);

        var hashtags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var tag = match.Groups[1].Value;
                // Validate length (1-50 characters)
                if (tag.Length >= 1 && tag.Length <= 50)
                {
                    hashtags.Add(tag.ToLowerInvariant());
                }
            }
        }

        return await Task.FromResult(hashtags.ToList());
    }

    public async Task<Hashtag> CreateHashtag(string tagName)
    {
        var normalizedTag = tagName.ToLowerInvariant();

        var hashtag = new Hashtag
        {
            TagName = normalizedTag
        };

        _dbContext.Hashtags.Add(hashtag);
        await _dbContext.SaveChangesAsync();

        return hashtag;
    }

    public async Task LinkCheepToHashtag(int cheepId, int hashtagId)
    {
        var cheep = await _dbContext.Cheeps
            .Include(c => c.Hashtags)
            .FirstOrDefaultAsync(c => c.CheepId == cheepId);

        var hashtag = await _dbContext.Hashtags
            .FirstOrDefaultAsync(h => h.HashtagId == hashtagId);

        if (cheep == null || hashtag == null)
        {
            return;
        }

        if (!cheep.Hashtags.Contains(hashtag))
        {
            cheep.Hashtags.Add(hashtag);
            await _dbContext.SaveChangesAsync();
        }
    }
}
