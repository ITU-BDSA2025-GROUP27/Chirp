using Chirp.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public required List<CheepDTO> Cheeps { get; set; }
    public HashSet<string> Following { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();

    [BindProperty]
    public CheepInputModel Input { get; set; } = new();

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    public async Task<ActionResult> OnGet(string author, [FromQuery] int page = 1)
    {
        var viewingUser = User.Identity?.Name;

        if (viewingUser != null && viewingUser == author)
        {
            // Viewing own timeline - show own cheeps + followed users cheeps
            var following = await _service.GetFollowing(author);
            var authors = following.Select(f => f.UserName).ToList();
            authors.Add(author);
            Cheeps = _service.GetCheepsFromAuthors(authors, page);
        }
        else
        {
            // Viewing someone elses timeline - show only their cheeps
            Cheeps = _service.GetCheepsFromAuthor(author, page);
        }

        Pagination = new PaginationViewModel
        {
            CurrentPage = page,
            CheepCount = Cheeps.Count,
            BaseUrl = $"/{author}"
        };

        await LoadFollowingAsync();
        return Page();
    }

    public async Task<ActionResult> OnPost(string author)
    {
        if (!ModelState.IsValid)
        {
            var viewingUser = User.Identity?.Name;

            if (viewingUser != null && viewingUser == author)
            {
                // Viewing own timeline - show own cheeps + followed users cheeps
                var following = await _service.GetFollowing(author);
                var authors = following.Select(f => f.UserName).ToList();
                authors.Add(author);
                Cheeps = _service.GetCheepsFromAuthors(authors, 1);
            }
            else
            {
                // Viewing someone elses timeline - show only their cheeps
                Cheeps = _service.GetCheepsFromAuthor(author, 1);
            }

            await LoadFollowingAsync();
            return Page();
        }

        var userName = User.Identity?.Name;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (userName != null && userEmail != null)
        {
            await _service.CreateCheep(userName, userEmail, Input.Text);
        }

        return RedirectToPage("UserTimeline", new { author });
    }

    public async Task<IActionResult> OnPostFollow(string author, string authorName)
    {
        var userName = User.Identity?.Name;
        if (userName != null && authorName != null)
        {
            await _service.FollowAuthor(userName, authorName);
        }
        return RedirectToPage("UserTimeline", new { author });
    }

    public async Task<IActionResult> OnPostUnfollow(string author, string authorName)
    {
        var userName = User.Identity?.Name;
        if (userName != null && authorName != null)
        {
            await _service.UnfollowAuthor(userName, authorName);
        }
        return RedirectToPage("UserTimeline", new { author });
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
