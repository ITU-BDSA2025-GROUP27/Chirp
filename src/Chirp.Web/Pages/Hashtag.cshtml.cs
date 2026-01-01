using Chirp.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

/// <summary>
/// Hashtag timeline - shows all cheeps containing a specific hashtag.
/// </summary>
public class HashtagModel : PageModel
{
    private readonly ICheepService _cheepService;
    private readonly IAuthorService _authorService;

    public required List<CheepDTO> Cheeps { get; set; }
    public HashSet<string> Following { get; set; } = new();
    public required string HashtagName { get; set; }
    public PaginationViewModel Pagination { get; set; } = new();

    public HashtagModel(ICheepService cheepService, IAuthorService authorService)
    {
        _cheepService = cheepService;
        _authorService = authorService;
    }

    public async Task<ActionResult> OnGet(string tagName, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return Redirect("/");
        }

        HashtagName = tagName;
        Cheeps = _cheepService.GetCheepsByHashtag(tagName, page);
        Pagination = new PaginationViewModel
        {
            CurrentPage = page,
            CheepCount = Cheeps.Count,
            BaseUrl = $"/hashtag/{tagName}"
        };
        await LoadFollowingAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostFollow(string tagName, string authorName)
    {
        var userName = User.Identity?.Name;
        if (userName != null && authorName != null)
        {
            await _authorService.FollowAuthor(userName, authorName);
        }
        return RedirectToPage("Hashtag", new { tagName });
    }

    public async Task<IActionResult> OnPostUnfollow(string tagName, string authorName)
    {
        var userName = User.Identity?.Name;
        if (userName != null && authorName != null)
        {
            await _authorService.UnfollowAuthor(userName, authorName);
        }
        return RedirectToPage("Hashtag", new { tagName });
    }

    private async Task LoadFollowingAsync()
    {
        if (User.Identity?.IsAuthenticated == true && User.Identity.Name != null)
        {
            var followedUsers = await _authorService.GetFollowing(User.Identity.Name);

            foreach (var user in followedUsers)
            {
                Following.Add(user.UserName);
            }
        }
    }
}
