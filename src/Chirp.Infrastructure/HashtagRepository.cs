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
        var query = (from ch in _dbContext.CheepHashtags
                    join h in _dbContext.Hashtags on ch.HashtagId equals h.HashtagId
                    where ch.CheepId == cheepId
                    select new HashtagDTO(h.TagName));

        var result = await query.ToListAsync();
        return result;
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
        // Check if link already exists to avoid duplicates
        var exists = await _dbContext.CheepHashtags
            .AnyAsync(ch => ch.CheepId == cheepId && ch.HashtagId == hashtagId);

        if (!exists)
        {
            var cheepHashtag = new CheepHashtag
            {
                CheepId = cheepId,
                HashtagId = hashtagId
            };

            _dbContext.CheepHashtags.Add(cheepHashtag);
            await _dbContext.SaveChangesAsync();
        }
    }
}
