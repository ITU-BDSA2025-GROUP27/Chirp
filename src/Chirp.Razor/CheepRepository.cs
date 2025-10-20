using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor;

public class CheepRepository : ICheepRepository
{
    private readonly ChirpDBContext _dbContext;

    public CheepRepository(ChirpDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Cheep>> GetCheeps(int page)
    {
        var query = (from cheep in _dbContext.Cheeps
                    orderby cheep.TimeStamp descending
                    select cheep)
                    .Include(c => c.Author)
                    .Skip((page - 1) * 32)
                    .Take(32);

        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<Cheep>> GetCheepsByAuthor(string author, int page)
    {
        var query = (from cheep in _dbContext.Cheeps
                    where cheep.Author.Name == author
                    orderby cheep.TimeStamp descending
                    select cheep)
                    .Include(c => c.Author)
                    .Skip((page - 1) * 32)
                    .Take(32);

        var result = await query.ToListAsync();
        return result;
    }
}
