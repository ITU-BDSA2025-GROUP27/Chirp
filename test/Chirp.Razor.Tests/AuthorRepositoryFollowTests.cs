using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Tests;

public class AuthorRepositoryFollowTests
{
    [Fact]
    public async Task FollowAuthor_AddsFollowRelationship_WhenBothAuthorsExist()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var follower = new Author
        {
            UserName = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        var followed = new Author
        {
            UserName = "Author2",
            Email = "author2@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(follower, followed);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        await repository.FollowAuthor("Author1", "Author2");

        // Assert
        var isFollowing = await repository.IsFollowing("Author1", "Author2");
        Assert.True(isFollowing);
    }

    [Fact]
    public async Task FollowAuthor_DoesNotAllowSelfFollow()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var author = new Author
        {
            UserName = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        await repository.FollowAuthor("Author1", "Author1");

        // Assert
        var isFollowing = await repository.IsFollowing("Author1", "Author1");
        Assert.False(isFollowing);
    }

    [Fact]
    public async Task FollowAuthor_PreventsDuplicateFollow()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var follower = new Author
        {
            UserName = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        var followed = new Author
        {
            UserName = "Author2",
            Email = "author2@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(follower, followed);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        await repository.FollowAuthor("Author1", "Author2");
        await repository.FollowAuthor("Author1", "Author2"); // Follow again

        // Assert
        var following = await repository.GetFollowing("Author1");
        Assert.Single(following);
        Assert.Equal("Author2", following[0].UserName);
    }

    [Fact]
    public async Task UnfollowAuthor_RemovesFollowRelationship()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var follower = new Author
        {
            UserName = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        var followed = new Author
        {
            UserName = "Author2",
            Email = "author2@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(follower, followed);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);
        await repository.FollowAuthor("Author1", "Author2");

        // Act
        await repository.UnfollowAuthor("Author1", "Author2");

        // Assert
        var isFollowing = await repository.IsFollowing("Author1", "Author2");
        Assert.False(isFollowing);
    }

    [Fact]
    public async Task UnfollowAuthor_DoesNotThrow_WhenNotFollowing()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var follower = new Author
        {
            UserName = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        var followed = new Author
        {
            UserName = "Author2",
            Email = "author2@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(follower, followed);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        await repository.UnfollowAuthor("Author1", "Author2");

        // Assert
        var isFollowing = await repository.IsFollowing("Author1", "Author2");
        Assert.False(isFollowing);
    }

    [Fact]
    public async Task IsFollowing_ReturnsTrue_WhenFollowing()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var follower = new Author
        {
            UserName = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        var followed = new Author
        {
            UserName = "Author2",
            Email = "author2@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(follower, followed);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);
        await repository.FollowAuthor("Author1", "Author2");

        // Act
        var result = await repository.IsFollowing("Author1", "Author2");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsFollowing_ReturnsFalse_WhenNotFollowing()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var follower = new Author
        {
            UserName = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        var followed = new Author
        {
            UserName = "Author2",
            Email = "author2@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(follower, followed);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        var result = await repository.IsFollowing("Author1", "Author2");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetFollowing_ReturnsListOfFollowedAuthors()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var author1 = new Author
        {
            UserName = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        var author2 = new Author
        {
            UserName = "Author2",
            Email = "author2@example.com",
            Cheeps = new List<Cheep>()
        };

        var author3 = new Author
        {
            UserName = "Author3",
            Email = "author3@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(author1, author2, author3);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);
        await repository.FollowAuthor("Author1", "Author2");
        await repository.FollowAuthor("Author1", "Author3");

        // Act
        var following = await repository.GetFollowing("Author1");

        // Assert
        Assert.Equal(2, following.Count);
        Assert.Contains(following, f => f.UserName == "Author2");
        Assert.Contains(following, f => f.UserName == "Author3");
    }

    [Fact]
    public async Task GetFollowing_ReturnsEmptyList_WhenNotFollowingAnyone()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var author = new Author
        {
            UserName = "Author1",
            Email = "author1@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        IAuthorRepository repository = new AuthorRepository(context);

        // Act
        var following = await repository.GetFollowing("Author1");

        // Assert
        Assert.Empty(following);
    }
}
