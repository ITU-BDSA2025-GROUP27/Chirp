using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;

public class Hashtag
{
    public int HashtagId { get; set; }

    [Required]
    [StringLength(50)]
    public required string TagName { get; set; }
    public ICollection<Cheep> Cheeps { get; set; } = new List<Cheep>();
}
