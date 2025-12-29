using Chirp.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public required List<CheepDTO> Cheeps { get; set; }
    public HashSet<string> Following { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();

    [BindProperty]
    public CheepInputModel Input { get; set; } = new();

    public PublicModel(ICheepService service)
    {
        _service = service;
    }

    public async Task<ActionResult> OnGet([FromQuery] int page = 1)
    {
        Cheeps = _service.GetCheeps(page);
        Pagination = new PaginationViewModel
        {
            CurrentPage = page,
            CheepCount = Cheeps.Count,
            BaseUrl = "/"
        };
        await LoadFollowingAsync();
        return Page();
    }

    public async Task<ActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            Cheeps = _service.GetCheeps(1);
            await LoadFollowingAsync();
            return Page();
        }

        var userName = User.Identity?.Name;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (userName != null && userEmail != null)
        {
            await _service.CreateCheep(userName, userEmail, Input.Text);
        }

        return Redirect("/");
    }

    public async Task<IActionResult> OnPostFollow(string authorName)
    {
        var userName = User.Identity?.Name;
        if (userName != null && authorName != null)
        {
            await _service.FollowAuthor(userName, authorName);
        }
        return Redirect("/");
    }

    public async Task<IActionResult> OnPostUnfollow(string authorName)
    {
        var userName = User.Identity?.Name;
        if (userName != null && authorName != null)
        {
            await _service.UnfollowAuthor(userName, authorName);
        }
        return Redirect("/");
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
