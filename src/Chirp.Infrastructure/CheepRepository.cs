using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class CheepRepository : ICheepRepository
{
    private readonly ChirpDBContext _dbContext;
    private readonly IAuthorRepository _authorRepository;
    private readonly IHashtagRepository _hashtagRepository;

    public CheepRepository(ChirpDBContext dbContext, IAuthorRepository authorRepository, IHashtagRepository hashtagRepository)
    {
        _dbContext = dbContext;
        _authorRepository = authorRepository;
        _hashtagRepository = hashtagRepository;
    }

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

    public async Task<List<CheepDTO>> GetCheepsByHashtag(string tagName, int page)
    {
        var normalizedTag = tagName.ToLowerInvariant();

        var query = (from ch in _dbContext.CheepHashtags
                    join h in _dbContext.Hashtags on ch.HashtagId equals h.HashtagId
                    join c in _dbContext.Cheeps on ch.CheepId equals c.CheepId
                    where h.TagName == normalizedTag
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

    public async Task CreateCheep(string authorName, string authorEmail, string text)
    {
        // Find or create author
        var author = await _authorRepository.FindAuthorByName(authorName);
        if (author == null)
        {
            author = new Author
            {
                UserName = authorName,
                Email = authorEmail,
                Cheeps = new List<Cheep>()
            };
            await _authorRepository.CreateAuthor(author);
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
