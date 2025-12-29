using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Tests;

public class HashtagIntegrationTests
{
    [Fact]
    public async Task CreateCheepWithHashtag_AutomaticallyCreatesAndLinksHashtag()
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

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "This is a test #hashtag");

        // Assert - Hashtag was created
        var hashtag = await context.Hashtags.FirstOrDefaultAsync(h => h.TagName == "hashtag");
        Assert.NotNull(hashtag);

        // Assert - Cheep was created and linked to hashtag
        var cheeps = await context.Cheeps.ToListAsync();
        Assert.Single(cheeps);

        var link = await context.CheepHashtags
            .FirstOrDefaultAsync(ch => ch.CheepId == cheeps[0].CheepId && ch.HashtagId == hashtag.HashtagId);
        Assert.NotNull(link);
    }

    [Fact]
    public async Task CreateCheepWithMultipleHashtags_CreatesAndLinksAllHashtags()
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

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Test #hashtag1 and #hashtag2 and #hashtag3");

        // Assert - All three hashtags were created
        var hashtags = await context.Hashtags.ToListAsync();
        Assert.Equal(3, hashtags.Count);
        Assert.Contains(hashtags, h => h.TagName == "hashtag1");
        Assert.Contains(hashtags, h => h.TagName == "hashtag2");
        Assert.Contains(hashtags, h => h.TagName == "hashtag3");

        // Assert - Cheep is linked to all three hashtags
        var cheep = await context.Cheeps.FirstAsync();
        var links = await context.CheepHashtags.Where(ch => ch.CheepId == cheep.CheepId).ToListAsync();
        Assert.Equal(3, links.Count);
    }

    [Fact]
    public async Task CreateMultipleCheepsWithSameHashtag_ReusesExistingHashtag()
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

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act - Create three cheeps with the same hashtag
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "First cheep with #test");
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Second cheep with #test");
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Third cheep with #test");

        // Assert - Only one hashtag was created
        var hashtags = await context.Hashtags.Where(h => h.TagName == "test").ToListAsync();
        Assert.Single(hashtags);

        // Assert - All three cheeps are linked to the same hashtag
        var cheeps = await context.Cheeps.ToListAsync();
        Assert.Equal(3, cheeps.Count);

        var links = await context.CheepHashtags.Where(ch => ch.HashtagId == hashtags[0].HashtagId).ToListAsync();
        Assert.Equal(3, links.Count);
    }

    [Fact]
    public async Task GetCheepsByHashtag_ReturnsOnlyCheepsWithThatHashtag()
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

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Create cheeps with different hashtags
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "First cheep with #test");
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Second cheep with #another");
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Third cheep with #test");

        // Act
        var result = cheepService.GetCheepsByHashtag("test", 1);

        // Assert - Only two cheeps with #test are returned
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Contains("#test", c.Text));
    }

    [Fact]
    public async Task GetCheepsByHashtag_IsCaseInsensitive()
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

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Create cheep with hashtag
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Test #TestHashtag");

        // Act - Query with different case
        var result = cheepService.GetCheepsByHashtag("testhashtag", 1);

        // Assert
        Assert.Single(result);
        Assert.Contains("#TestHashtag", result[0].Text);
    }

    [Fact]
    public async Task GetCheepsByHashtag_ReturnsEmptyList_WhenHashtagDoesNotExist()
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

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Test cheep without hashtag");

        // Act
        var result = cheepService.GetCheepsByHashtag("nonexistent", 1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCheepsByHashtag_ReturnsCheepsInDescendingOrder()
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

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Create cheeps with delays to ensure different timestamps
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Old cheep with #test");
        await Task.Delay(100);
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Middle cheep with #test");
        await Task.Delay(100);
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "New cheep with #test");

        // Act
        var result = cheepService.GetCheepsByHashtag("test", 1);

        // Assert - Most recent cheep should be first
        Assert.Equal(3, result.Count);
        Assert.Contains("New cheep", result[0].Text);
        Assert.Contains("Middle cheep", result[1].Text);
        Assert.Contains("Old cheep", result[2].Text);
    }

    [Fact]
    public async Task GetCheepsByHashtag_ReturnsPaginatedResults()
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

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Create 35 cheeps with the same hashtag (page size is 32)
        for (int i = 0; i < 35; i++)
        {
            await cheepService.CreateCheep("TestAuthor", "test@example.com", $"Cheep {i} with #test");
        }

        // Act
        var page1 = cheepService.GetCheepsByHashtag("test", 1);
        var page2 = cheepService.GetCheepsByHashtag("test", 2);

        // Assert
        Assert.Equal(32, page1.Count);
        Assert.Equal(3, page2.Count);
    }

    [Fact]
    public async Task CreateCheepWithDuplicateHashtagsInText_LinksOnlyOnce()
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

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act - Create cheep with duplicate hashtag
        await cheepService.CreateCheep("TestAuthor", "test@example.com", "Test #hashtag and #hashtag again");

        // Assert - Only one hashtag was created
        var hashtags = await context.Hashtags.ToListAsync();
        Assert.Single(hashtags);

        // Assert - Cheep is linked only once to the hashtag
        var cheep = await context.Cheeps.FirstAsync();
        var links = await context.CheepHashtags.Where(ch => ch.CheepId == cheep.CheepId).ToListAsync();
        Assert.Single(links);
    }

    [Fact]
    public async Task MultipleAuthorsCanUseSameHashtag()
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

        context.Authors.AddRange(author1, author2);
        await context.SaveChangesAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act - Both authors use the same hashtag
        await cheepService.CreateCheep("Author1", "author1@example.com", "Author1's cheep with #test");
        await cheepService.CreateCheep("Author2", "author2@example.com", "Author2's cheep with #test");

        // Assert - Only one hashtag was created
        var hashtags = await context.Hashtags.Where(h => h.TagName == "test").ToListAsync();
        Assert.Single(hashtags);

        // Assert - Both authors' cheeps appear when searching for the hashtag
        var result = cheepService.GetCheepsByHashtag("test", 1);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Author == "Author1");
        Assert.Contains(result, c => c.Author == "Author2");
    }
}
