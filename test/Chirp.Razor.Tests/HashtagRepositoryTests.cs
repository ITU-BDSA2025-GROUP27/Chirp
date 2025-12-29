using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Tests;

public class HashtagRepositoryTests
{
    [Fact]
    public async Task FindHashtagByName_ReturnsHashtag_WhenHashtagExists()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var hashtag = new Hashtag
        {
            TagName = "testhashtag"
        };

        context.Hashtags.Add(hashtag);
        await context.SaveChangesAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        // Act
        var result = await repository.FindHashtagByName("testhashtag");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testhashtag", result.TagName);
    }

    [Fact]
    public async Task FindHashtagByName_ReturnsNull_WhenHashtagDoesNotExist()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        // Act
        var result = await repository.FindHashtagByName("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindHashtagByName_IsCaseInsensitive()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var hashtag = new Hashtag
        {
            TagName = "testhashtag"
        };

        context.Hashtags.Add(hashtag);
        await context.SaveChangesAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        // Act
        var result = await repository.FindHashtagByName("TestHashtag");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testhashtag", result.TagName);
    }

    [Fact]
    public async Task GetHashtagsForCheep_ReturnsHashtags_WhenCheepHasHashtags()
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

        var cheep = new Cheep
        {
            Text = "This is a test #hashtag1 and #hashtag2",
            TimeStamp = DateTime.Now,
            Author = author
        };

        var hashtag1 = new Hashtag { TagName = "hashtag1" };
        var hashtag2 = new Hashtag { TagName = "hashtag2" };

        context.Authors.Add(author);
        context.Cheeps.Add(cheep);
        context.Hashtags.AddRange(hashtag1, hashtag2);
        await context.SaveChangesAsync();

        context.CheepHashtags.Add(new CheepHashtag { CheepId = cheep.CheepId, HashtagId = hashtag1.HashtagId });
        context.CheepHashtags.Add(new CheepHashtag { CheepId = cheep.CheepId, HashtagId = hashtag2.HashtagId });
        await context.SaveChangesAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        // Act
        var result = await repository.GetHashtagsForCheep(cheep.CheepId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, h => h.TagName == "hashtag1");
        Assert.Contains(result, h => h.TagName == "hashtag2");
    }

    [Fact]
    public async Task GetHashtagsForCheep_ReturnsEmptyList_WhenCheepHasNoHashtags()
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

        var cheep = new Cheep
        {
            Text = "This is a test without hashtags",
            TimeStamp = DateTime.Now,
            Author = author
        };

        context.Authors.Add(author);
        context.Cheeps.Add(cheep);
        await context.SaveChangesAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        // Act
        var result = await repository.GetHashtagsForCheep(cheep.CheepId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHashtagNamesInText_ExtractsHashtags_FromText()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        var text = "This is a test #hashtag1 and #hashtag2 message";

        // Act
        var result = await repository.GetHashtagNamesInText(text);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("hashtag1", result);
        Assert.Contains("hashtag2", result);
    }

    [Fact]
    public async Task GetHashtagNamesInText_ReturnsEmptyList_WhenNoHashtags()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        var text = "This is a test message without hashtags";

        // Act
        var result = await repository.GetHashtagNamesInText(text);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHashtagNamesInText_RemovesDuplicates()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        var text = "This is a test #hashtag1 and #hashtag1 again #HASHTAG1";

        // Act
        var result = await repository.GetHashtagNamesInText(text);

        // Assert
        Assert.Single(result);
        Assert.Contains("hashtag1", result);
    }

    [Fact]
    public async Task GetHashtagNamesInText_NormalizesToLowerCase()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        var text = "This is a test #TestHashtag";

        // Act
        var result = await repository.GetHashtagNamesInText(text);

        // Assert
        Assert.Single(result);
        Assert.Contains("testhashtag", result);
    }

    [Fact]
    public async Task GetHashtagNamesInText_IgnoresHashtagsLongerThan50Characters()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        var text = "This is a test #" + new string('a', 51) + " #validhashtag";

        // Act
        var result = await repository.GetHashtagNamesInText(text);

        // Assert
        Assert.Single(result);
        Assert.Contains("validhashtag", result);
    }

    [Fact]
    public async Task CreateHashtag_CreatesNewHashtag()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        // Act
        var result = await repository.CreateHashtag("newhashtag");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newhashtag", result.TagName);
        var savedHashtag = await context.Hashtags.FirstOrDefaultAsync(h => h.TagName == "newhashtag");
        Assert.NotNull(savedHashtag);
    }

    [Fact]
    public async Task CreateHashtag_NormalizesToLowerCase()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        // Act
        var result = await repository.CreateHashtag("NewHashtag");

        // Assert
        Assert.Equal("newhashtag", result.TagName);
    }

    [Fact]
    public async Task LinkCheepToHashtag_CreatesLink()
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

        var cheep = new Cheep
        {
            Text = "Test cheep",
            TimeStamp = DateTime.Now,
            Author = author
        };

        var hashtag = new Hashtag { TagName = "testhashtag" };

        context.Authors.Add(author);
        context.Cheeps.Add(cheep);
        context.Hashtags.Add(hashtag);
        await context.SaveChangesAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        // Act
        await repository.LinkCheepToHashtag(cheep.CheepId, hashtag.HashtagId);

        // Assert
        var link = await context.CheepHashtags
            .FirstOrDefaultAsync(ch => ch.CheepId == cheep.CheepId && ch.HashtagId == hashtag.HashtagId);
        Assert.NotNull(link);
    }

    [Fact]
    public async Task LinkCheepToHashtag_DoesNotCreateDuplicateLink()
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

        var cheep = new Cheep
        {
            Text = "Test cheep",
            TimeStamp = DateTime.Now,
            Author = author
        };

        var hashtag = new Hashtag { TagName = "testhashtag" };

        context.Authors.Add(author);
        context.Cheeps.Add(cheep);
        context.Hashtags.Add(hashtag);
        await context.SaveChangesAsync();

        IHashtagRepository repository = new HashtagRepository(context);

        // Act - Link twice
        await repository.LinkCheepToHashtag(cheep.CheepId, hashtag.HashtagId);
        await repository.LinkCheepToHashtag(cheep.CheepId, hashtag.HashtagId);

        // Assert - Should only have one link
        var links = await context.CheepHashtags
            .Where(ch => ch.CheepId == cheep.CheepId && ch.HashtagId == hashtag.HashtagId)
            .ToListAsync();
        Assert.Single(links);
    }
}
