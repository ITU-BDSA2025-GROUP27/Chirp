using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;

public class CheepInputModel
{
    [Required]
    [StringLength(160, ErrorMessage = "Maximum length is {1}")]
    public string Text { get; set; } = string.Empty;
}
