using Chirp.Core;

namespace Chirp.Infrastructure;

public class CheepService : ICheepService
{
    private readonly ICheepRepository _cheepRepository;
    private readonly IAuthorRepository _authorRepository;

    public CheepService(ICheepRepository cheepRepository, IAuthorRepository authorRepository)
    {
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
    }

    /// <inheritdoc />
    public List<CheepDTO> GetCheeps(int page = 1)
    {
        return _cheepRepository.GetCheeps(page).Result;
    }

    /// <inheritdoc />
    public List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1)
    {
        return _cheepRepository.GetCheepsByAuthor(author, page).Result;
    }

    /// <inheritdoc />
    public List<CheepDTO> GetCheepsFromAuthors(List<string> authors, int page = 1)
    {
        return _cheepRepository.GetCheepsByAuthors(authors, page).Result;
    }

    /// <inheritdoc />
    public List<CheepDTO> GetCheepsByHashtag(string tagName, int page = 1)
    {
        return _cheepRepository.GetCheepsByHashtag(tagName, page).Result;
    }

    /// <inheritdoc />
    public async Task CreateCheep(string authorName, string authorEmail, string text)
    {
        // Service orchestrates: find or create author
        var authorDTO = await _authorRepository.FindAuthorByName(authorName);
        if (authorDTO == null)
        {
            var newAuthor = new Author
            {
                UserName = authorName,
                Email = authorEmail,
                Cheeps = new List<Cheep>()
            };
            await _authorRepository.CreateAuthor(newAuthor);
        }

        // Then tell repository to create the cheep
        await _cheepRepository.CreateCheep(authorName, text);
    }
}
