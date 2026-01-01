using Microsoft.AspNetCore.Identity;

namespace Chirp.Core;

/// <summary>
/// Author entity - extends IdentityUser with cheeps and follow relationships.
/// </summary>
public class Author : IdentityUser<int>
{
    /// <summary>
    /// Primary key for the author.
    /// </summary>
    public int AuthorId { get; set; }

    /// <summary>
    /// Collection of cheeps created by this author.
    /// </summary>
    public required ICollection<Cheep> Cheeps { get; set; }

    /// <summary>
    /// Authors that this author follows.
    /// </summary>
    public ICollection<Author> Following { get; set; } = new List<Author>();

    /// <summary>
    /// Authors who follow this author.
    /// </summary>
    public ICollection<Author> Followers { get; set; } = new List<Author>();
}
