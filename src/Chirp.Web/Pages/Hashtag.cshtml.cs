using Chirp.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class HashtagModel : PageModel
{
    private readonly ICheepService _service;
    public required List<CheepDTO> Cheeps { get; set; }
    public HashSet<string> Following { get; set; } = new();
    public required string HashtagName { get; set; }
    public PaginationViewModel Pagination { get; set; } = new();

    public HashtagModel(ICheepService service)
    {
        _service = service;
    }

    public async Task<ActionResult> OnGet(string tagName, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return Redirect("/");
        }

        HashtagName = tagName;
        Cheeps = _service.GetCheepsByHashtag(tagName, page);
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
            await _service.FollowAuthor(userName, authorName);
        }
        return RedirectToPage("Hashtag", new { tagName });
    }

    public async Task<IActionResult> OnPostUnfollow(string tagName, string authorName)
    {
        var userName = User.Identity?.Name;
        if (userName != null && authorName != null)
        {
            await _service.UnfollowAuthor(userName, authorName);
        }
        return RedirectToPage("Hashtag", new { tagName });
    }

    private async Task LoadFollowingAsync()
    {
        if (User.Identity?.IsAuthenticated == true && User.Identity.Name != null)
        {
            var followedUsers = await _service.GetFollowing(User.Identity.Name);

            foreach (var user in followedUsers)
            {
                Following.Add(user.UserName);
            }
        }
    }
}
