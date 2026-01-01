using Chirp.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

/// <summary>
/// Public timeline - shows all cheeps from all users.
/// </summary>
public class PublicModel : PageModel
{
    private readonly ICheepService _cheepService;
    private readonly IAuthorService _authorService;

    public required List<CheepDTO> Cheeps { get; set; }
    public HashSet<string> Following { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();

    [BindProperty]
    public CheepInputModel Input { get; set; } = new();

    public PublicModel(ICheepService cheepService, IAuthorService authorService)
    {
        _cheepService = cheepService;
        _authorService = authorService;
    }

    public async Task<ActionResult> OnGet([FromQuery] int page = 1)
    {
        Cheeps = _cheepService.GetCheeps(page);
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
            Cheeps = _cheepService.GetCheeps(1);
            await LoadFollowingAsync();
            return Page();
        }

        var userName = User.Identity?.Name;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (userName != null && userEmail != null)
        {
            await _cheepService.CreateCheep(userName, userEmail, Input.Text);
        }

        return Redirect("/");
    }

    public async Task<IActionResult> OnPostFollow(string authorName)
    {
        var userName = User.Identity?.Name;
        if (userName != null && authorName != null)
        {
            await _authorService.FollowAuthor(userName, authorName);
        }
        return Redirect("/");
    }

    public async Task<IActionResult> OnPostUnfollow(string authorName)
    {
        var userName = User.Identity?.Name;
        if (userName != null && authorName != null)
        {
            await _authorService.UnfollowAuthor(userName, authorName);
        }
        return Redirect("/");
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
