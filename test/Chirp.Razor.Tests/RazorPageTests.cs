using Microsoft.AspNetCore.Mvc.Testing;

namespace Chirp.Razor.Tests;

public class RazorPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fixture;
    private readonly HttpClient _client;

    public RazorPageTests(WebApplicationFactory<Program> fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true, HandleCookies = true });
    }

    [Fact]
    public async void CanSeePublicTimeline()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content);
        Assert.Contains("Public Timeline", content);
    }

    [Fact]
    public async void PublicTimelineContainsSpecificCheeps()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        // Check for any author from the database
        Assert.Contains("Jacqualine Gilcoine", content);
    }

    [Theory]
    [InlineData("Helge", "Hello, BDSA students!")]
    [InlineData("Adrian", "Hej, velkommen til kurset.")]
    public async void CanSeePrivateTimeline(string author, string expectedMessage)
    {
        var response = await _client.GetAsync($"/{author}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content);
        Assert.Contains($"{author}'s Timeline", content);
        Assert.Contains(author, content);
        Assert.Contains(expectedMessage, content);
    }

    [Fact]
    public async void PaginationWorks()
    {
        var response = await _client.GetAsync("/?page=2");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content);
        Assert.Contains("Public Timeline", content);
    }

    [Fact]
    public async void PaginationWorksOnPrivateTimeline()
    {
        var response = await _client.GetAsync("/Jacqualine Gilcoine?page=2");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content);
    }
}
