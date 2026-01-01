using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;

/// <summary>
/// A cheep - max 160 characters.
/// </summary>
public class Cheep
{
    /// <summary>
    /// Primary key for the cheep.
    /// </summary>
    public int CheepId { get; set; }

    /// <summary>
    /// Content of the cheep (max 160 characters).
    /// </summary>
    [Required]
    [StringLength(160)]
    public required string Text { get; set; }

    /// <summary>
    /// When the cheep was created.
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Foreign key to the author.
    /// </summary>
    public int AuthorId { get; set; }

    /// <summary>
    /// The author who created this cheep.
    /// </summary>
    public required Author Author { get; set; }

    /// <summary>
    /// Hashtags extracted from this cheep's text.
    /// </summary>
    public ICollection<Hashtag> Hashtags { get; set; } = new List<Hashtag>();
}
