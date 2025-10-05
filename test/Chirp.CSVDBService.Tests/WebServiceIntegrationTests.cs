using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Chirp.CSVDBService.Tests;

public class WebServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCheeps_ReturnsOkStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/cheeps");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCheeps_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/cheeps");

        // Assert
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetCheeps_ReturnsListOfCheeps()
    {
        // Act
        var response = await _client.GetAsync("/cheeps");
        var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();

        // Assert
        Assert.NotNull(cheeps);
        Assert.IsType<List<Cheep>>(cheeps);
    }

    [Fact]
    public async Task PostCheep_ReturnsOkStatusCode()
    {
        // Arrange
        var cheep = new Cheep("testuser", "Test message", DateTimeOffset.Now.ToUnixTimeSeconds());

        // Act
        var response = await _client.PostAsJsonAsync("/cheep", cheep);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostCheep_CheepIsStoredAndRetrievable()
    {
        // Arrange
        var testMessage = $"Integration test message {Guid.NewGuid()}";
        var cheep = new Cheep("integrationtest", testMessage, DateTimeOffset.Now.ToUnixTimeSeconds());

        // Act
        await _client.PostAsJsonAsync("/cheep", cheep);
        var response = await _client.GetAsync("/cheeps");
        var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();

        // Assert
        Assert.NotNull(cheeps);
        Assert.Contains(cheeps, c => c.Message == testMessage);
    }
}

public record Cheep(string Author, string Message, long Timestamp);
