using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class CheepRepository : ICheepRepository
{
    private readonly ChirpDBContext _dbContext;
    private readonly IHashtagRepository _hashtagRepository;

    public CheepRepository(ChirpDBContext dbContext, IHashtagRepository hashtagRepository)
    {
        _dbContext = dbContext;
        _hashtagRepository = hashtagRepository;
    }

    /// <inheritdoc />
    public async Task<List<CheepDTO>> GetCheeps(int page)
    {
        var query = (from cheep in _dbContext.Cheeps
                    orderby cheep.TimeStamp descending
                    select new CheepDTO(
                        cheep.Author.UserName!,
                        cheep.Text,
                        cheep.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
                    .Skip((page - 1) * 32)
                    .Take(32);

        var result = await query.ToListAsync();
        return result;
    }

    /// <inheritdoc />
    public async Task<List<CheepDTO>> GetCheepsByAuthor(string author, int page)
    {
        var query = (from cheep in _dbContext.Cheeps
                    where cheep.Author.UserName == author
                    orderby cheep.TimeStamp descending
                    select new CheepDTO(
                        cheep.Author.UserName!,
                        cheep.Text,
                        cheep.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
                    .Skip((page - 1) * 32)
                    .Take(32);

        var result = await query.ToListAsync();
        return result;
    }

    /// <inheritdoc />
    public async Task<List<CheepDTO>> GetCheepsByAuthors(List<string> authors, int page)
    {
        var query = (from cheep in _dbContext.Cheeps
                    where authors.Contains(cheep.Author.UserName!)
                    orderby cheep.TimeStamp descending
                    select new CheepDTO(
                        cheep.Author.UserName!,
                        cheep.Text,
                        cheep.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
                    .Skip((page - 1) * 32)
                    .Take(32);

        var result = await query.ToListAsync();
        return result;
    }

    /// <inheritdoc />
    public async Task<List<CheepDTO>> GetCheepsByHashtag(string tagName, int page)
    {
        var normalizedTag = tagName.ToLowerInvariant();

        var query = (from h in _dbContext.Hashtags
                    where h.TagName == normalizedTag
                    from c in h.Cheeps
                    orderby c.TimeStamp descending
                    select new CheepDTO(
                        c.Author.UserName!,
                        c.Text,
                        c.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
                    .Skip((page - 1) * 32)
                    .Take(32);

        var result = await query.ToListAsync();
        return result;
    }

    /// <inheritdoc />
    public async Task CreateCheep(string authorName, string text)
    {
        var author = await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.UserName == authorName);

        if (author == null)
        {
            return;
        }

        // Create cheep
        var cheep = new Cheep
        {
            Text = text,
            TimeStamp = DateTime.Now,
            Author = author
        };

        _dbContext.Cheeps.Add(cheep);
        await _dbContext.SaveChangesAsync();

        // Extract and process hashtags
        var hashtagNames = await _hashtagRepository.GetHashtagNamesInText(text);
        foreach (var tagName in hashtagNames)
        {
            // Find or create hashtag
            var hashtag = await _hashtagRepository.FindHashtagByName(tagName);
            if (hashtag == null)
            {
                hashtag = await _hashtagRepository.CreateHashtag(tagName);
            }

            // Link cheep to hashtag
            await _hashtagRepository.LinkCheepToHashtag(cheep.CheepId, hashtag.HashtagId);
        }
    }
}
