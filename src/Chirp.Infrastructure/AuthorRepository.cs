using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class AuthorRepository : IAuthorRepository
{
    private readonly ChirpDBContext _dbContext;

    public AuthorRepository(ChirpDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Author?> FindAuthorByName(string name)
    {
        return await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.UserName == name);
    }

    public async Task<Author?> FindAuthorByEmail(string email)
    {
        return await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<bool> IsFollowing(string followerName, string followedName)
    {
        var follower = await _dbContext.Authors
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == followerName);

        if (follower == null) return false;

        return follower.Following.Any(a => a.UserName == followedName);
    }

    public async Task<List<string>> GetFollowing(string authorName)
    {
        var author = await _dbContext.Authors
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == authorName);

        if (author == null) return new List<string>();

        return author.Following.Select(a => a.UserName!).ToList();
    }

    public async Task CreateAuthor(Author author)
    {
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();
    }

    public async Task FollowAuthor(string followerName, string followedName)
    {
        var follower = await _dbContext.Authors
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == followerName);

        var followed = await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.UserName == followedName);

        if (follower == null || followed == null || followerName == followedName)
        {
            return;
        }

        if (!follower.Following.Contains(followed))
        {
            follower.Following.Add(followed);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UnfollowAuthor(string followerName, string followedName)
    {
        var follower = await _dbContext.Authors
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == followerName);

        var followed = await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.UserName == followedName);

        if (follower == null || followed == null)
        {
            return;
        }

        if (follower.Following.Contains(followed))
        {
            follower.Following.Remove(followed);
            await _dbContext.SaveChangesAsync();
        }
    }
}
