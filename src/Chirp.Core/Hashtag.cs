using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;

/// <summary>
/// A hashtag - max 50 characters.
/// </summary>
public class Hashtag
{
    /// <summary>
    /// Primary key for the hashtag.
    /// </summary>
    public int HashtagId { get; set; }

    /// <summary>
    /// Name of the hashtag.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string TagName { get; set; }

    /// <summary>
    /// Cheeps that contain this hashtag.
    /// </summary>
    public ICollection<Cheep> Cheeps { get; set; } = new List<Cheep>();
}
