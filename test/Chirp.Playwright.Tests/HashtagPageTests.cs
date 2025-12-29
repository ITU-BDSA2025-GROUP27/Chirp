using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Chirp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Chirp.Core;

namespace Chirp.Playwright.Tests;

[NonParallelizable]
[TestFixture]
public class HashtagPageTests : PageTest
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

        // Get all cheep IDs from test users
        var testCheepIds = testAuthors.SelectMany(a => a.Cheeps).Select(c => c.CheepId).ToList();

        // Remove CheepHashtag links
        var cheepHashtags = await context.CheepHashtags
            .Where(ch => testCheepIds.Contains(ch.CheepId))
            .ToListAsync();
        context.CheepHashtags.RemoveRange(cheepHashtags);

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
    public async Task CheepsWithHashtagsShowClickableLinks()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var testHashtag = $"test{Guid.NewGuid():N}";
        var cheepText = $"Test cheep with #{testHashtag}";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Create a cheep with a hashtag
        await Page.FillAsync("#Input_Text", cheepText);
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify hashtag link is rendered
        var hashtagLink = Page.Locator($"a[href='/hashtag/{testHashtag}']");
        await Expect(hashtagLink).ToBeVisibleAsync();
        var linkText = await hashtagLink.InnerTextAsync();
        Assert.That(linkText, Is.EqualTo($"#{testHashtag}"));
    }

    [Test]
    public async Task UserCanNavigateToHashtagPageByClickingHashtagLink()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var testHashtag = $"test{Guid.NewGuid():N}";
        var cheepText = $"Test cheep with #{testHashtag}";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Create a cheep with a hashtag
        await Page.FillAsync("#Input_Text", cheepText);
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Click the hashtag link
        var hashtagLink = Page.Locator($"a[href='/hashtag/{testHashtag}']").First;
        await hashtagLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify navigation to hashtag page
        var url = Page.Url;
        Assert.That(url, Does.Contain($"/hashtag/{testHashtag}"));
    }

    [Test]
    public async Task HashtagPageDisplaysCheepsWithThatHashtag()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var testHashtag = $"test{Guid.NewGuid():N}";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Create two cheeps with the same hashtag
        await Page.FillAsync("#Input_Text", $"First cheep with #{testHashtag}");
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.FillAsync("#Input_Text", $"Second cheep with #{testHashtag}");
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to hashtag page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/hashtag/{testHashtag}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify both cheeps are displayed
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain("First cheep with"));
        Assert.That(pageContent, Does.Contain("Second cheep with"));
        Assert.That(pageContent, Does.Contain($"#{testHashtag}"));
    }

    [Test]
    public async Task HashtagPageOnlyShowsCheepsWithMatchingHashtag()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var testHashtag1 = $"test{Guid.NewGuid():N}";
        var testHashtag2 = $"other{Guid.NewGuid():N}";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Create cheeps with different hashtags
        await Page.FillAsync("#Input_Text", $"Cheep with #{testHashtag1}");
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.FillAsync("#Input_Text", $"Cheep with #{testHashtag2}");
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to hashtag1 page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/hashtag/{testHashtag1}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify only cheeps with hashtag1 are displayed
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain($"#{testHashtag1}"));
        Assert.That(pageContent, Does.Not.Contain($"#{testHashtag2}"));
    }

    [Test]
    public async Task HashtagPageDisplaysHashtagNameInHeader()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var testHashtag = $"test{Guid.NewGuid():N}";
        _testUserEmails.Add(testEmail);

        // Register a new user and create a cheep
        await RegisterUser(testUserName, testEmail, testPassword);
        await Page.FillAsync("#Input_Text", $"Test cheep with #{testHashtag}");
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to hashtag page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/hashtag/{testHashtag}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify hashtag name is displayed in header
        await Expect(Page.Locator("h2")).ToContainTextAsync($"#{testHashtag}");
    }

    [Test]
    public async Task UnauthenticatedUserCanViewHashtagPage()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var testHashtag = $"test{Guid.NewGuid():N}";
        _testUserEmails.Add(testEmail);

        // Register a new user and create a cheep
        await RegisterUser(testUserName, testEmail, testPassword);
        await Page.FillAsync("#Input_Text", $"Public cheep with #{testHashtag}");
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Logout
        await Page.ClickAsync("text=logout");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Navigate to hashtag page as unauthenticated user
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/hashtag/{testHashtag}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify cheep is visible
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain("Public cheep with"));
        Assert.That(pageContent, Does.Contain($"#{testHashtag}"));
    }

    [Test]
    public async Task NonExistentHashtagPageShowsHashtagName()
    {
        var nonExistentHashtag = $"nonexistent{Guid.NewGuid():N}";

        // Navigate to hashtag page with non-existent hashtag
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/hashtag/{nonExistentHashtag}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify page loads and shows the hashtag name
        await Expect(Page.Locator("h2")).ToContainTextAsync($"#{nonExistentHashtag}");
    }

    [Test]
    public async Task CheepWithMultipleHashtagsShowsAllAsClickableLinks()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        var testHashtag1 = $"test1{Guid.NewGuid():N}";
        var testHashtag2 = $"test2{Guid.NewGuid():N}";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Create a cheep with multiple hashtags
        await Page.FillAsync("#Input_Text", $"Cheep with #{testHashtag1} and #{testHashtag2}");
        await Page.ClickAsync(".cheepbox input[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify both hashtag links are rendered
        var hashtagLink1 = Page.Locator($"a[href='/hashtag/{testHashtag1}']");
        var hashtagLink2 = Page.Locator($"a[href='/hashtag/{testHashtag2}']");
        await Expect(hashtagLink1).ToBeVisibleAsync();
        await Expect(hashtagLink2).ToBeVisibleAsync();

        // Click first hashtag and verify navigation
        await hashtagLink1.First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.That(Page.Url, Does.Contain($"/hashtag/{testHashtag1}"));

        // Go back and click second hashtag
        await Page.GoBackAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await hashtagLink2.First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.That(Page.Url, Does.Contain($"/hashtag/{testHashtag2}"));
    }
}
