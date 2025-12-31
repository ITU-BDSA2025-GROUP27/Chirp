using Chirp.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO.Compression;
using System.Security.Claims;

namespace Chirp.Web.Pages;

[Authorize]
public class AboutMeModel : PageModel
{
    private readonly ICheepService _cheepService;
    private readonly IAuthorService _authorService;
    private readonly IAuthorRepository _authorRepository;
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;

    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<AuthorDTO> Following { get; set; } = new();
    public List<CheepDTO> Cheeps { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();

    public AboutMeModel(ICheepService cheepService, IAuthorService authorService, IAuthorRepository authorRepository,
        UserManager<Author> userManager, SignInManager<Author> signInManager)
    {
        _cheepService = cheepService;
        _authorService = authorService;
        _authorRepository = authorRepository;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet([FromQuery] int page = 1)
    {
        if (User.Identity?.Name == null)
        {
            return RedirectToPage("/Public");
        }

        UserName = User.Identity.Name;
        Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

        Cheeps = _cheepService.GetCheepsFromAuthor(UserName, page);
        Pagination = new PaginationViewModel
        {
            CurrentPage = page,
            CheepCount = Cheeps.Count,
            BaseUrl = "/aboutme"
        };

        Following = await _authorService.GetFollowing(UserName);

        return Page();
    }

    public async Task<IActionResult> OnPostDownloadData()
    {
        if (User.Identity?.Name == null)
        {
            return RedirectToPage("/Public");
        }

        var userName = User.Identity.Name;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

        var following = await _authorService.GetFollowing(userName);
        var allCheeps = await GetAllUserCheeps(userName);

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var userInfoEntry = archive.CreateEntry("user_info.txt");
            using (var writer = new StreamWriter(userInfoEntry.Open()))
            {
                await writer.WriteLineAsync("USER INFORMATION");
                await writer.WriteLineAsync("================");
                await writer.WriteLineAsync($"Username: {userName}");
                await writer.WriteLineAsync($"Email: {email}");
                await writer.WriteLineAsync($"Data exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }

            var followingEntry = archive.CreateEntry("following.csv");
            using (var writer = new StreamWriter(followingEntry.Open()))
            {
                await writer.WriteLineAsync("Username");
                foreach (var followedUser in following)
                {
                    await writer.WriteLineAsync(followedUser.UserName);
                }
            }

            var cheepsEntry = archive.CreateEntry("cheeps.csv");
            using (var writer = new StreamWriter(cheepsEntry.Open()))
            {
                await writer.WriteLineAsync("Author,Text,Timestamp");
                foreach (var cheep in allCheeps)
                {
                    var escapedText = cheep.Text.Replace("\"", "\"\"");
                    await writer.WriteLineAsync($"{cheep.Author},\"{escapedText}\",{cheep.TimeStamp}");
                }
            }
        }

        memoryStream.Position = 0;
        var fileName = $"{userName}_chirp_data_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        return File(memoryStream.ToArray(), "application/zip", fileName);
    }

    private async Task<List<CheepDTO>> GetAllUserCheeps(string userName)
    {
        var allCheeps = new List<CheepDTO>();
        int page = 1;
        List<CheepDTO> pageCheeps;

        do
        {
            pageCheeps = _cheepService.GetCheepsFromAuthor(userName, page);
            allCheeps.AddRange(pageCheeps);
            page++;
        } while (pageCheeps.Count == 32);

        return allCheeps;
    }

    public async Task<IActionResult> OnPostForgetMe()
    {
        if (User.Identity?.Name == null)
        {
            return RedirectToPage("/Public");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Public");
        }

        // Delete the user account (this will remove all associated data)
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            // Log the user out
            await _signInManager.SignOutAsync();

            // Redirect to the public timeline
            return RedirectToPage("/Public");
        }

        // If deletion failed, stay on the page
        return Page();
    }
}
