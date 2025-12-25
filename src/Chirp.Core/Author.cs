using Microsoft.AspNetCore.Identity;

namespace Chirp.Core;

public class Author : IdentityUser<int>
{
    public int AuthorId { get; set; }
    public required ICollection<Cheep> Cheeps { get; set; }
    public ICollection<Author> Following { get; set; } = new List<Author>();
    public ICollection<Author> Followers { get; set; } = new List<Author>();
}
