using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Chirp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Chirp.Core;

namespace Chirp.Playwright.Tests;

[NonParallelizable]
[TestFixture]
public class ForgetMeTests : PageTest
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
    public async Task ForgetMeDeletesUserFromDatabase()
    {
        var testUserName = $"TestUser{Guid.NewGuid():N}";
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var testPassword = "TestPassword123!";
        _testUserEmails.Add(testEmail);

        // Register a new user
        await RegisterUser(testUserName, testEmail, testPassword);

        // Verify user exists in database before deletion
        using (var context = CreateDbContext())
        {
            var userExists = await context.Authors.AnyAsync(a => a.Email == testEmail);
            Assert.That(userExists, Is.True, "User should exist before deletion");
        }

        // Navigate to About Me page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/aboutme");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Handle the confirmation dialog
        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync();
        };

        // Click Forget Me button
        await Page.ClickAsync("button.btn-danger:has-text('Forget me!')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify user is deleted from database
        using (var context = CreateDbContext())
        {
            var userExists = await context.Authors.AnyAsync(a => a.Email == testEmail);
            Assert.That(userExists, Is.False, "User should be deleted from database");
        }

        // Remove from cleanup list since it's already deleted
        _testUserEmails.Remove(testEmail);
    }

    [Test]
    public async Task UserIsLoggedOutAfterForgetMe()
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

        // Handle the confirmation dialog
        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync();
        };

        // Click Forget Me button
        await Page.ClickAsync("button.btn-danger:has-text('Forget me!')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify user cannot access About Me page anymore
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/aboutme");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var url = Page.Url;
        Assert.That(url, Does.Contain("/Identity/Account/Login").Or.Not.Contains("/aboutme"),
            "Should not have access to About Me page after deletion");

        // Remove from cleanup list since it's already deleted
        _testUserEmails.Remove(testEmail);
    }

    [Test]
    public async Task ForgetMeDeletesUserCheeps()
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

        // Verify the cheep is stored in the database
        using (var context = CreateDbContext())
        {
            var cheepExists = await context.Cheeps.AnyAsync(c => c.Text == cheepText);
            Assert.That(cheepExists, Is.True, "Cheep should exist before user deletion");
        }

        // Navigate to About Me page
        await Page.GotoAsync($"{PlaywrightTestBase.BaseUrl}/aboutme");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Handle the confirmation dialog
        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync();
        };

        // Click Forget Me button
        await Page.ClickAsync("button.btn-danger:has-text('Forget me!')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the cheep is deleted from database
        using (var context = CreateDbContext())
        {
            var cheepExists = await context.Cheeps.AnyAsync(c => c.Text == cheepText);
            Assert.That(cheepExists, Is.False, "User's cheeps should be deleted when user is deleted");
        }

        // Remove from cleanup list since it's already deleted
        _testUserEmails.Remove(testEmail);
    }

    [Test]
    public async Task ForgetMeShowsConfirmationDialog()
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

        // Set up dialog handler to verify it appears
        var dialogShown = false;
        Page.Dialog += async (_, dialog) =>
        {
            dialogShown = true;
            Assert.That(dialog.Message, Does.Contain("sure").IgnoreCase,
                "Dialog should ask for confirmation");
            await dialog.DismissAsync(); // Dismiss to cancel deletion
        };

        // Click Forget Me button
        await Page.ClickAsync("button.btn-danger:has-text('Forget me!')");
        await Page.WaitForTimeoutAsync(500); // Give time for dialog to appear

        // Verify confirmation dialog was shown
        Assert.That(dialogShown, Is.True, "Confirmation dialog should be shown");

        // Verify user still exists in database (because we dismissed the dialog)
        using (var context = CreateDbContext())
        {
            var userExists = await context.Authors.AnyAsync(a => a.Email == testEmail);
            Assert.That(userExists, Is.True, "User should still exist after canceling deletion");
        }
    }

    [Test]
    public async Task UserIsRedirectedToPublicTimelineAfterForgetMe()
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

        // Handle the confirmation dialog
        Page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync();
        };

        // Click Forget Me button
        await Page.ClickAsync("button.btn-danger:has-text('Forget me!')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify user is redirected to public timeline
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain("Public Timeline"),
            "Should be redirected to public timeline after deletion");

        // Remove from cleanup list since it's already deleted
        _testUserEmails.Remove(testEmail);
    }
}
