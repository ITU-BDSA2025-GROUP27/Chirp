using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Tests;

public class AuthorRepositoryTests
{
    [Fact]
    public async Task FindAuthorByName_ReturnsAuthorDTO_WhenAuthorExists()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var author = new Author
        {
            UserName = "TestAuthor",
            Email = "test@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        var result = await repository.FindAuthorByName("TestAuthor");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestAuthor", result.UserName);
    }

    [Fact]
    public async Task FindAuthorByName_ReturnsNull_WhenAuthorDoesNotExist()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        var result = await repository.FindAuthorByName("NonExistentAuthor");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindAuthorByEmail_ReturnsAuthorDTO_WhenAuthorExists()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var author = new Author
        {
            UserName = "TestAuthor",
            Email = "test@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        var result = await repository.FindAuthorByEmail("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestAuthor", result.UserName);
    }

    [Fact]
    public async Task FindAuthorByEmail_ReturnsNull_WhenAuthorDoesNotExist()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        var result = await repository.FindAuthorByEmail("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAuthor_AddsAuthorToDatabase()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        var newAuthor = new Author
        {
            UserName ="NewAuthor",
            Email = "new@example.com",
            Cheeps = new List<Cheep>()
        };

        // Act
        await repository.CreateAuthor(newAuthor);

        // Assert
        var result = await context.Authors.FirstOrDefaultAsync(a => a.UserName == "NewAuthor");
        Assert.NotNull(result);
        Assert.Equal("NewAuthor", result.UserName);
        Assert.Equal("new@example.com", result.Email);
    }
}
