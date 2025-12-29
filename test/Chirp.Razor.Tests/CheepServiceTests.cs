using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Tests;

public class CheepServiceTests
{
    [Fact]
    public void UnixTimeStampToDateTime_ConvertsCorrectly()
    {
        // Arrange
        long unixTime = 1690892208; // Used in test data for Helge's cheep
        DateTimeOffset expectedDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        string expectedString = expectedDateTime.ToString("MM/dd/yy H:mm:ss");

        // Act
        string result = ConvertUnixTimeStamp(unixTime);

        // Assert
        Assert.Equal(expectedString, result);
    }

    [Fact]
    public async Task CreateCheep_CreatesNewCheep_ThroughService()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act
        await cheepService.CreateCheep("TestUser", "test@example.com", "Hello, World!");

        // Assert
        var cheeps = cheepService.GetCheeps(1);
        Assert.Single(cheeps);
        Assert.Equal("TestUser", cheeps[0].Author);
        Assert.Equal("Hello, World!", cheeps[0].Text);
    }

    [Fact]
    public async Task CreateCheep_CheepAppearsOnPublicTimeline()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act
        await cheepService.CreateCheep("Author1", "author1@example.com", "First cheep");
        await cheepService.CreateCheep("Author2", "author2@example.com", "Second cheep");

        // Assert
        var publicCheeps = cheepService.GetCheeps(1);
        Assert.Equal(2, publicCheeps.Count);
    }

    [Fact]
    public async Task CreateCheep_CheepAppearsOnAuthorTimeline()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "My cheep");
        await cheepService.CreateCheep("OtherAuthor", "other@example.com", "Other cheep");

        // Assert
        var authorCheeps = cheepService.GetCheepsFromAuthor("TestAuthor", 1);
        Assert.Single(authorCheeps);
        Assert.Equal("TestAuthor", authorCheeps[0].Author);
        Assert.Equal("My cheep", authorCheeps[0].Text);
    }

    // Helper method that mimics CheepService.UnixTimeStampToDateTimeString
    private static string ConvertUnixTimeStamp(double unixTimeStamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }
}
