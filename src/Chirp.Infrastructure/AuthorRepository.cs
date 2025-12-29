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

    public async Task<AuthorDTO?> FindAuthorByName(string name)
    {
        var author = await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.UserName == name);

        return author != null ? new AuthorDTO(author.UserName!) : null;
    }

    public async Task<AuthorDTO?> FindAuthorByEmail(string email)
    {
        var author = await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.Email == email);

        return author != null ? new AuthorDTO(author.UserName!) : null;
    }

    public async Task<bool> IsFollowing(string followerName, string followedName)
    {
        var follower = await _dbContext.Authors
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == followerName);

        if (follower == null) return false;

        return follower.Following.Any(a => a.UserName == followedName);
    }

    public async Task<List<AuthorDTO>> GetFollowing(string authorName)
    {
        var query = (from author in _dbContext.Authors
                    where author.UserName == authorName
                    from followedAuthor in author.Following
                    select new AuthorDTO(followedAuthor.UserName!));

        var result = await query.ToListAsync();
        return result;
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
