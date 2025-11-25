using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Chirp.Playwright.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class UITests : PageTest
{
    private Process? _serverProcess;
    private const string BaseUrl = "http://localhost:5273";

    [SetUp]
    public async Task Init()
    {
        // Start the Chirp.Web server
        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project ../../src/Chirp.Web/Chirp.Web.csproj --no-build",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        _serverProcess.Start();

        // Wait for server to be ready
        await Task.Delay(3000);
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill();
            _serverProcess.Dispose();
        }
    }

    [Test]
    public async Task UnauthenticatedUserDoesNotSeeCheepBoxOnPublicTimeline()
    {
        await Page.GotoAsync(BaseUrl);

        // Expect to see the Public Timeline heading
        await Expect(Page.Locator("h2")).ToContainTextAsync("Public Timeline");

        // Verify that the cheep box is NOT present for unauthenticated users
        var cheepbox = Page.Locator(".cheepbox");
        await Expect(cheepbox).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task UnauthenticatedUserDoesNotSeeCheepBoxOnUserTimeline()
    {
        await Page.GotoAsync($"{BaseUrl}/Helge");

        // Expect to see the user's timeline heading
        await Expect(Page.Locator("h2")).ToContainTextAsync("Helge's Timeline");

        // Verify that the cheep box is NOT present for unauthenticated users
        var cheepbox = Page.Locator(".cheepbox");
        await Expect(cheepbox).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task PublicTimelineDisplaysCheeps()
    {
        await Page.GotoAsync(BaseUrl);

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
        await Page.GotoAsync(BaseUrl);

        // Click on the first author link in the message list
        var firstAuthorLink = Page.Locator("#messagelist strong a").First;
        await firstAuthorLink.ClickAsync();

        // Expect to be on a user's timeline
        await Expect(Page.Locator("h2")).ToContainTextAsync("Timeline");
    }
}
