using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;

public class Author
{
    public int AuthorId { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Email { get; set; }

    public required ICollection<Cheep> Cheeps { get; set; }
}
