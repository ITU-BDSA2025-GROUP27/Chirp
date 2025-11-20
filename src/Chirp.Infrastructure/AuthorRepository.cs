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

    public async Task CreateAuthor(Author author)
    {
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();
    }
}
