using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Tests;

public class CheepServiceFollowTests
{
    [Fact]
    public async Task GetCheepsFromAuthors_IncludesAllAuthorsInList()
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

        var author1Cheep = new Cheep
        {
            Text = "Cheep by Author1",
            TimeStamp = DateTime.Now,
            Author = author1
        };

        var author2Cheep = new Cheep
        {
            Text = "Cheep by Author2",
            TimeStamp = DateTime.Now,
            Author = author2
        };

        context.Authors.AddRange(author1, author2);
        context.Cheeps.AddRange(author1Cheep, author2Cheep);
        await context.SaveChangesAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act
        var cheeps = cheepService.GetCheepsFromAuthors(new List<string> { "Author1", "Author2" }, 1);

        // Assert
        Assert.Equal(2, cheeps.Count);
        Assert.Contains(cheeps, c => c.Author == "Author1");
        Assert.Contains(cheeps, c => c.Author == "Author2");
    }

    [Fact]
    public async Task GetCheepsFromAuthors_ShowsOwnCheepsAndFollowedUsersCheeps()
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

        var author1Cheep = new Cheep
        {
            Text = "Cheep by Author1",
            TimeStamp = DateTime.Now,
            Author = author1
        };

        var author2Cheep = new Cheep
        {
            Text = "Cheep by Author2",
            TimeStamp = DateTime.Now,
            Author = author2
        };

        var author3Cheep = new Cheep
        {
            Text = "Cheep by Author3",
            TimeStamp = DateTime.Now,
            Author = author3
        };

        context.Authors.AddRange(author1, author2, author3);
        context.Cheeps.AddRange(author1Cheep, author2Cheep, author3Cheep);
        await context.SaveChangesAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        await cheepService.FollowAuthor("Author1", "Author2");

        // Act
        var following = await cheepService.GetFollowing("Author1");
        var authors = new List<string>(following) { "Author1" };
        var cheeps = cheepService.GetCheepsFromAuthors(authors, 1);

        // Assert
        Assert.Equal(2, cheeps.Count);
        Assert.Contains(cheeps, c => c.Author == "Author1");
        Assert.Contains(cheeps, c => c.Author == "Author2");
        Assert.DoesNotContain(cheeps, c => c.Author == "Author3");
    }

    [Fact]
    public async Task GetCheepsFromAuthors_DoesNotShowUnfollowedUsersCheeps()
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

        var author1Cheep = new Cheep
        {
            Text = "Cheep by Author1",
            TimeStamp = DateTime.Now,
            Author = author1
        };

        var author2Cheep = new Cheep
        {
            Text = "Cheep by Author2",
            TimeStamp = DateTime.Now,
            Author = author2
        };

        context.Authors.AddRange(author1, author2);
        context.Cheeps.AddRange(author1Cheep, author2Cheep);
        await context.SaveChangesAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        await cheepService.FollowAuthor("Author1", "Author2");
        await cheepService.UnfollowAuthor("Author1", "Author2");

        // Act
        var following = await cheepService.GetFollowing("Author1");
        var authors = new List<string>(following) { "Author1" };
        var cheeps = cheepService.GetCheepsFromAuthors(authors, 1);

        // Assert
        Assert.Single(cheeps);
        Assert.Equal("Author1", cheeps[0].Author);
    }

    [Fact]
    public async Task OtherUserTimeline_ShowsOnlyThatUsersCheeps()
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

        var author2Cheep = new Cheep
        {
            Text = "Cheep by Author2",
            TimeStamp = DateTime.Now,
            Author = author2
        };

        var author3Cheep = new Cheep
        {
            Text = "Cheep by Author3",
            TimeStamp = DateTime.Now,
            Author = author3
        };

        context.Authors.AddRange(author1, author2, author3);
        context.Cheeps.AddRange(author2Cheep, author3Cheep);
        await context.SaveChangesAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        await cheepService.FollowAuthor("Author2", "Author3");

        // Act
        var author2Cheeps = cheepService.GetCheepsFromAuthor("Author2", 1);

        // Assert
        Assert.Single(author2Cheeps);
        Assert.Equal("Author2", author2Cheeps[0].Author);
        Assert.DoesNotContain(author2Cheeps, c => c.Author == "Author3");
    }
}
