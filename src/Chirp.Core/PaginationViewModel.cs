namespace Chirp.Core;

public class PaginationViewModel
{
    public int CurrentPage { get; set; }
    public int CheepCount { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
}
