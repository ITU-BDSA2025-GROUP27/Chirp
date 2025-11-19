using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Tests;

public class CheepRepositoryTests
{
    [Fact]
    public async Task GetCheeps_ReturnsCheeps_WhenDatabaseHasData()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var author = new Author
        {
            Name = "TestAuthor",
            Email = "test@example.com",
            Cheeps = new List<Cheep>()
        };

        var cheep = new Cheep
        {
            Text = "This is a test cheep",
            TimeStamp = DateTime.UtcNow,
            Author = author
        };

        context.Authors.Add(author);
        context.Cheeps.Add(cheep);
        await context.SaveChangesAsync();

        ICheepRepository repository = new CheepRepository(context);

        // Act
        var result = await repository.GetCheeps(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("TestAuthor", result[0].Author);
        Assert.Equal("This is a test cheep", result[0].Text);
    }

    [Fact]
    public async Task GetCheepsByAuthor_ReturnsOnlyAuthorsCheeps()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var author1 = new Author
        {
            Name = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        var author2 = new Author
        {
            Name = "Author2",
            Email = "author2@example.com",
            Cheeps = new List<Cheep>()
        };

        var cheep1 = new Cheep
        {
            Text = "Cheep by Author1",
            TimeStamp = DateTime.UtcNow,
            Author = author1
        };

        var cheep2 = new Cheep
        {
            Text = "Cheep by Author2",
            TimeStamp = DateTime.UtcNow,
            Author = author2
        };

        context.Authors.AddRange(author1, author2);
        context.Cheeps.AddRange(cheep1, cheep2);
        await context.SaveChangesAsync();

        ICheepRepository repository = new CheepRepository(context);

        // Act
        var result = await repository.GetCheepsByAuthor("Author1", 1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Author1", result[0].Author);
        Assert.Equal("Cheep by Author1", result[0].Text);
    }

    [Fact]
    public async Task GetCheeps_ReturnsCheepsInDescendingOrder()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var author = new Author
        {
            Name = "TestAuthor",
            Email = "test@example.com",
            Cheeps = new List<Cheep>()
        };

        var oldCheep = new Cheep
        {
            Text = "Old cheep",
            TimeStamp = DateTime.UtcNow.AddDays(-1),
            Author = author
        };

        var newCheep = new Cheep
        {
            Text = "New cheep",
            TimeStamp = DateTime.UtcNow,
            Author = author
        };

        context.Authors.Add(author);
        context.Cheeps.AddRange(oldCheep, newCheep);
        await context.SaveChangesAsync();

        ICheepRepository repository = new CheepRepository(context);

        // Act
        var result = await repository.GetCheeps(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("New cheep", result[0].Text);
        Assert.Equal("Old cheep", result[1].Text);
    }

    [Fact]
    public async Task GetCheeps_ReturnsPaginatedResults()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var author = new Author
        {
            Name = "TestAuthor",
            Email = "test@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.Add(author);

        // Add 35 cheeps to test pagination (page size is 32)
        for (int i = 0; i < 35; i++)
        {
            context.Cheeps.Add(new Cheep
            {
                Text = $"Cheep {i}",
                TimeStamp = DateTime.UtcNow.AddMinutes(-i),
                Author = author
            });
        }

        await context.SaveChangesAsync();

        ICheepRepository repository = new CheepRepository(context);

        // Act
        var page1 = await repository.GetCheeps(1);
        var page2 = await repository.GetCheeps(2);

        // Assert
        Assert.Equal(32, page1.Count);
        Assert.Equal(3, page2.Count);
    }
}
