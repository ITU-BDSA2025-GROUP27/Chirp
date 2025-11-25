using Chirp.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public required List<CheepDTO> Cheeps { get; set; }

    [BindProperty]
    public CheepInputModel Input { get; set; } = new();

    public PublicModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet([FromQuery] int page = 1)
    {
        Cheeps = _service.GetCheeps(page);
        return Page();
    }

    public async Task<ActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            Cheeps = _service.GetCheeps(1);
            return Page();
        }

        var userName = User.Identity?.Name;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (userName != null && userEmail != null)
        {
            await _service.CreateCheep(userName, userEmail, Input.Text);
        }

        return RedirectToPage("Public");
    }
}
