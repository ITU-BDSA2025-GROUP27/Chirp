using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Chirp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Chirp.Core;

namespace Chirp.Playwright.Tests;

[NonParallelizable]
[TestFixture]
public class AboutMeTests : PageTest
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

        // Find all test users created during this test
        var testAuthors = await context.Authors
            .Where(a => a.Email != null && _testUserEmails.Contains(a.Email))
            .Include(a => a.Cheeps)
            .ToListAsync();

        // Remove their cheeps first (foreign key constraint)
        foreach (var author in testAuthors)
        {
            context.Cheeps.RemoveRange(author.Cheeps);
        }

        // Remove the test users
        context.Authors.RemoveRange(testAuthors);

        await context.SaveChangesAsync();
        _testUserEmails.Clear();
    }

    private async Task RegisterUser(string userName, string email, string password)
    {
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/Identity/Account/Register");
        await Page.FillAsync("input[id='Input_UserName']", userName);
        await Page.FillAsync("input[id='Input_Email']", email);
        await Page.FillAsync("input[id='Input_Password']", password);
        await Page.FillAsync("input[id='Input_ConfirmPassword']", password);
        await Page.ClickAsync("button[id='registerSubmit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task AuthenticatedUserCanSeeTheirInformationOnAboutMePage()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Navigate to About Me page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/aboutme");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify user information is displayed
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain("About Me"));
        Assert.That(pageContent, Does.Contain("User Information"));
        Assert.That(pageContent, Does.Contain(testUserName));
        Assert.That(pageContent, Does.Contain(testEmail));
    }

    [Test]
    public async Task AboutMePageShowsFollowingSection()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Navigate to About Me page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/aboutme");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify following section is displayed
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain("Following"));
        Assert.That(pageContent, Does.Contain("You are not following anyone yet").Or.Contains("users)"));
    }

    [Test]
    public async Task UserCheepsAppearOnAboutMePage()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var cheepText = $"Test cheep {Guid.NewGuid():N}";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Fill in the cheep box and submit
        await Page.FillAsync("#Input_Text", cheepText);
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to About Me page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/aboutme");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the cheep appears on About Me page
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain("My Cheeps"));
        Assert.That(pageContent, Does.Contain(cheepText));
    }

    [Test]
    public async Task DownloadDataButtonIsVisibleOnAboutMePage()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Navigate to About Me page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/aboutme");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify download data button is visible
        var downloadButton = Page.Locator("button:has-text('Download All My Data')");
        await Expect(downloadButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task ForgetMeButtonIsVisibleOnAboutMePage()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Navigate to About Me page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/aboutme");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify Forget Me button is visible
        var forgetMeButton = Page.Locator("button.btn-danger:has-text('Forget me!')");
        await Expect(forgetMeButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task UnauthenticatedUserCannotAccessAboutMePage()
    {
        // Navigate to About Me page without logging in
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/aboutme");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify user is redirected to login page
        var url = Page.Url;
        Assert.That(url, Does.Contain("/Identity/Account/Login").Or.Not.Contains("/aboutme"));
    }
}
