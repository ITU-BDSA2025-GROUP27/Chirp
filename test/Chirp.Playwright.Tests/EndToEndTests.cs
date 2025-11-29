using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Chirp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Chirp.Core;

namespace Chirp.Playwright.Tests;

[NonParallelizable]
[TestFixture]
public class EndToEndTests : PageTest
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

    [Test]
    public async Task UserCanRegisterAndSeeCheepBox()
    {
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        _testUserEmails.Add(testEmail);

        // Navigate to register page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/Identity/Account/Register");

        // Fill in registration form
        await Page.FillAsync("input[id='Input_Email']", testEmail);
        await Page.FillAsync("input[id='Input_Password']", testPassword);
        await Page.FillAsync("input[id='Input_ConfirmPassword']", testPassword);

        // Submit registration
        await Page.ClickAsync("button[id='registerSubmit']");

        // Verify cheep box is now visible
        var cheepbox = Page.Locator(".cheepbox");
        await Expect(cheepbox).ToBeVisibleAsync();
    }

    [Test]
    public async Task UserCanSendCheepAndItAppearsOnTimeline()
    {
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var cheepText = $"Test cheep {Guid.NewGuid():N}";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/Identity/Account/Register");
        await Page.FillAsync("input[id='Input_Email']", testEmail);
        await Page.FillAsync("input[id='Input_Password']", testPassword);
        await Page.FillAsync("input[id='Input_ConfirmPassword']", testPassword);
        await Page.ClickAsync("button[id='registerSubmit']");

        // Fill in the cheep box and submit
        await Page.FillAsync("#Input_Text", cheepText);
        await Page.ClickAsync(".cheepbox input[type='submit']");

        // Wait for page to reload
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the cheep appears on the page
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain(cheepText));
    }

    [Test]
    public async Task UserCanSendCheepAndItIsStoredInDatabase()
    {
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var cheepText = $"Database test cheep {Guid.NewGuid():N}";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/Identity/Account/Register");
        await Page.FillAsync("input[id='Input_Email']", testEmail);
        await Page.FillAsync("input[id='Input_Password']", testPassword);
        await Page.FillAsync("input[id='Input_ConfirmPassword']", testPassword);
        await Page.ClickAsync("button[id='registerSubmit']");

        // Fill in the cheep box and submit
        await Page.FillAsync("#Input_Text", cheepText);
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the cheep is stored in the database
        using var context = CreateDbContext();
        var cheepInDb = await context.Cheeps
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Text == cheepText);

        Assert.That(cheepInDb, Is.Not.Null, "Cheep should be stored in database");
        Assert.That(cheepInDb!.Author.Email, Is.EqualTo(testEmail));
    }

    [Test]
    public async Task CheepWithMoreThan160CharactersIsTruncatedTo160()
    {
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var longCheepText = new string('a', 161);
        _testUserEmails.Add(testEmail);

        // Register a new user
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/Identity/Account/Register");
        await Page.FillAsync("input[id='Input_Email']", testEmail);
        await Page.FillAsync("input[id='Input_Password']", testPassword);
        await Page.FillAsync("input[id='Input_ConfirmPassword']", testPassword);
        await Page.ClickAsync("button[id='registerSubmit']");

        // Fill in the cheep box with text longer than 160 characters
        var cheepInput = Page.Locator("#Input_Text");
        await cheepInput.ClickAsync();
        await Page.Keyboard.TypeAsync(longCheepText);

        // Verify input is truncated to 160 characters
        var inputValue = await cheepInput.InputValueAsync();
        Assert.That(inputValue.Length, Is.EqualTo(160));
        Assert.That(inputValue, Is.EqualTo(longCheepText.Substring(0, 160)));
    }
}
