namespace Chirp.Core;

public interface IAuthorRepository
{
    // Queries
    Task<Author?> FindAuthorByName(string name);
    Task<Author?> FindAuthorByEmail(string email);

    // Commands
    Task CreateAuthor(Author author);
}
