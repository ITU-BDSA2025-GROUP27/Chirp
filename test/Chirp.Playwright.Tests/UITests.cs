using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Chirp.Infrastructure;
using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Playwright.Tests;

[NonParallelizable]
[TestFixture]
public class UITests : PageTest
{
    private readonly List<string> _testUserEmails = new();

    private ChirpDBContext CreateDbContext()
    {
        var solutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
        var dbPath = Path.Combine(solutionDir, "src/Chirp.Web/chirp.db");
        var optionsBuilder = new DbContextOptionsBuilder<ChirpDBContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        return new ChirpDBContext(optionsBuilder.Options);
    }

    [TearDown]
    public async Task CleanupTestData()
    {
        if (_testUserEmails.Count == 0) return;

        using var context = CreateDbContext();

        var testAuthors = await context.Authors
            .Where(a => a.Email != null && _testUserEmails.Contains(a.Email))
            .Include(a => a.Cheeps)
            .ToListAsync();

        foreach (var author in testAuthors)
        {
            context.Cheeps.RemoveRange(author.Cheeps);
        }

        context.Authors.RemoveRange(testAuthors);
        await context.SaveChangesAsync();
        _testUserEmails.Clear();
    }

    [Test]
    public async Task UnauthenticatedUserDoesNotSeeCheepBoxOnPublicTimeline()
    {
        await Page.GotoAsync(PlaywrightTestBase.BaseUrl);

        // Expect to see the Public Timeline heading
        await Expect(Page.Locator("h2")).ToContainTextAsync("Public Timeline");

        // Verify that the cheep box is NOT present for unauthenticated users
        var cheepbox = Page.Locator(".cheepbox");
        await Expect(cheepbox).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task UnauthenticatedUserDoesNotSeeCheepBoxOnUserTimeline()
    {
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/Helge");

        // Expect to see the user's timeline heading
        await Expect(Page.Locator("h2")).ToContainTextAsync("Helge's Timeline");

        // Verify that the cheep box is NOT present for unauthenticated users
        var cheepbox = Page.Locator(".cheepbox");
        await Expect(cheepbox).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task AuthenticatedUserSeesCheepBoxAfterLogin()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"uitest{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/Identity/Account/Register");
        await Page.FillAsync("input[id='Input_UserName']", testUserName);
        await Page.FillAsync("input[id='Input_Email']", testEmail);
        await Page.FillAsync("input[id='Input_Password']", testPassword);
        await Page.FillAsync("input[id='Input_ConfirmPassword']", testPassword);
        await Page.ClickAsync("button[id='registerSubmit']");

        // Verify cheep box IS visible for authenticated users
        var cheepbox = Page.Locator(".cheepbox");
        await Expect(cheepbox).ToBeVisibleAsync();

        // Verify the input field for text is present
        await Expect(Page.Locator("#Input_Text")).ToBeVisibleAsync();
        // Verify the submit button is present
        await Expect(Page.Locator(".cheepbox input[type='submit']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task PublicTimelineDisplaysCheeps()
    {
        await Page.GotoAsync(PlaywrightTestBase.BaseUrl);

        // Expect to see cheeps in the message list
        var messagelist = Page.Locator("#messagelist");
        await Expect(messagelist).ToBeVisibleAsync();

        // Verify that at least one cheep is displayed
        var cheepItems = Page.Locator("#messagelist li");
        await Expect(cheepItems.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task CanNavigateToUserTimelineFromPublicTimeline()
    {
        await Page.GotoAsync(PlaywrightTestBase.BaseUrl);

        // Click on the first author link in the message list
        var firstAuthorLink = Page.Locator("#messagelist strong a").First;
        await firstAuthorLink.ClickAsync();

        // Expect to be on a user's timeline
        await Expect(Page.Locator("h2")).ToContainTextAsync("Timeline");
    }
}
