using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Tests;

// Integration tests for follow/unfollow functionality based on the Anna, Bella, Cheryl scenario from session 11.
// Written with assistance from Claude.
public class FollowIntegrationTests
{
    [Fact]
    public async Task AnnaFollowsBellaAndCheryl_TimelinesAreCorrect()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        // Create the three users
        var anna = new Author
        {
            UserName = "Anna",
            Email = "anna@example.com",
            Cheeps = new List<Cheep>()
        };

        var bella = new Author
        {
            UserName = "Bella",
            Email = "bella@example.com",
            Cheeps = new List<Cheep>()
        };

        var cheryl = new Author
        {
            UserName = "Cheryl",
            Email = "cheryl@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(anna, bella, cheryl);
        await context.SaveChangesAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Bella follows Cheryl (initial state)
        await cheepService.FollowAuthor("Bella", "Cheryl");

        // Anna sends a cheep
        await cheepService.CreateCheep("Anna", "anna@example.com", "Anna's first cheep");

        // Bella sends some cheeps
        await cheepService.CreateCheep("Bella", "bella@example.com", "Bella's cheep 1");
        await cheepService.CreateCheep("Bella", "bella@example.com", "Bella's cheep 2");

        // Cheryl sends some cheeps
        await cheepService.CreateCheep("Cheryl", "cheryl@example.com", "Cheryl's cheep 1");
        await cheepService.CreateCheep("Cheryl", "cheryl@example.com", "Cheryl's cheep 2");

        // Act - Anna follows both Bella and Cheryl
        await cheepService.FollowAuthor("Anna", "Bella");
        await cheepService.FollowAuthor("Anna", "Cheryl");

        // Assert - Anna's timeline shows her own cheeps + Bella's + Cheryl's
        var annaFollowing = await cheepService.GetFollowing("Anna");
        var annaTimelineAuthors = new List<string>(annaFollowing) { "Anna" };
        var annaTimeline = cheepService.GetCheepsFromAuthors(annaTimelineAuthors, 1);

        Assert.Equal(5, annaTimeline.Count); // 1 from Anna + 2 from Bella + 2 from Cheryl
        Assert.Contains(annaTimeline, c => c.Author == "Anna");
        Assert.Contains(annaTimeline, c => c.Author == "Bella");
        Assert.Contains(annaTimeline, c => c.Author == "Cheryl");

        // Assert - When Anna visits Bella's timeline, she only sees Bella's cheeps
        var bellaTimeline = cheepService.GetCheepsFromAuthor("Bella", 1);
        Assert.Equal(2, bellaTimeline.Count);
        Assert.All(bellaTimeline, c => Assert.Equal("Bella", c.Author));

        // Assert - Anna cannot follow herself
        await cheepService.FollowAuthor("Anna", "Anna");
        var annaSelfFollow = await cheepService.IsFollowing("Anna", "Anna");
        Assert.False(annaSelfFollow);

        // Assert - Bella's timeline shows her own cheeps + Cheryl's cheeps
        var bellaFollowing = await cheepService.GetFollowing("Bella");
        var bellaTimelineAuthors = new List<string>(bellaFollowing) { "Bella" };
        var bellaOwnTimeline = cheepService.GetCheepsFromAuthors(bellaTimelineAuthors, 1);

        Assert.Equal(4, bellaOwnTimeline.Count); // 2 from Bella + 2 from Cheryl
        Assert.Contains(bellaOwnTimeline, c => c.Author == "Bella");
        Assert.Contains(bellaOwnTimeline, c => c.Author == "Cheryl");
        Assert.DoesNotContain(bellaOwnTimeline, c => c.Author == "Anna");
    }

    [Fact]
    public async Task AnnaUnfollowsBella_TimelineIsUpdated()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        // Create the three users
        var anna = new Author
        {
            UserName = "Anna",
            Email = "anna@example.com",
            Cheeps = new List<Cheep>()
        };

        var bella = new Author
        {
            UserName = "Bella",
            Email = "bella@example.com",
            Cheeps = new List<Cheep>()
        };

        var cheryl = new Author
        {
            UserName = "Cheryl",
            Email = "cheryl@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(anna, bella, cheryl);
        await context.SaveChangesAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Create cheeps
        await cheepService.CreateCheep("Anna", "anna@example.com", "Anna's cheep");
        await cheepService.CreateCheep("Bella", "bella@example.com", "Bella's cheep");
        await cheepService.CreateCheep("Cheryl", "cheryl@example.com", "Cheryl's cheep");

        // Bella follows Cheryl, Anna follows both Bella and Cheryl
        await cheepService.FollowAuthor("Bella", "Cheryl");
        await cheepService.FollowAuthor("Anna", "Bella");
        await cheepService.FollowAuthor("Anna", "Cheryl");

        // Verify initial state - Anna sees all three
        var annaFollowing = await cheepService.GetFollowing("Anna");
        var annaTimelineAuthors = new List<string>(annaFollowing) { "Anna" };
        var annaTimelineBefore = cheepService.GetCheepsFromAuthors(annaTimelineAuthors, 1);
        Assert.Equal(3, annaTimelineBefore.Count);

        // Act - Anna unfollows Bella
        await cheepService.UnfollowAuthor("Anna", "Bella");

        // Assert - Anna's timeline now shows only her own cheeps and Cheryl's
        var annaFollowingAfter = await cheepService.GetFollowing("Anna");
        var annaTimelineAuthorsAfter = new List<string>(annaFollowingAfter) { "Anna" };
        var annaTimelineAfter = cheepService.GetCheepsFromAuthors(annaTimelineAuthorsAfter, 1);

        Assert.Equal(2, annaTimelineAfter.Count);
        Assert.Contains(annaTimelineAfter, c => c.Author == "Anna");
        Assert.Contains(annaTimelineAfter, c => c.Author == "Cheryl");
        Assert.DoesNotContain(annaTimelineAfter, c => c.Author == "Bella");

        // Assert - Anna is now only following Cheryl
        var annaFollowingList = await cheepService.GetFollowing("Anna");
        Assert.Single(annaFollowingList);
        Assert.Equal("Cheryl", annaFollowingList[0]);

        // Assert - Bella still follows Cheryl (Anna's action didn't affect Bella)
        var bellaFollowing = await cheepService.GetFollowing("Bella");
        Assert.Single(bellaFollowing);
        Assert.Equal("Cheryl", bellaFollowing[0]);
    }

    [Fact]
    public async Task ViewingOtherUsersTimeline_ShowsOnlyTheirCheeps()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var anna = new Author
        {
            UserName = "Anna",
            Email = "anna@example.com",
            Cheeps = new List<Cheep>()
        };

        var bella = new Author
        {
            UserName = "Bella",
            Email = "bella@example.com",
            Cheeps = new List<Cheep>()
        };

        var cheryl = new Author
        {
            UserName = "Cheryl",
            Email = "cheryl@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(anna, bella, cheryl);
        await context.SaveChangesAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Create cheeps
        await cheepService.CreateCheep("Anna", "anna@example.com", "Anna's cheep");
        await cheepService.CreateCheep("Bella", "bella@example.com", "Bella's cheep 1");
        await cheepService.CreateCheep("Bella", "bella@example.com", "Bella's cheep 2");
        await cheepService.CreateCheep("Cheryl", "cheryl@example.com", "Cheryl's cheep");

        // Bella follows Cheryl
        await cheepService.FollowAuthor("Bella", "Cheryl");

        // Act & Assert - When Anna (or anyone) views Bella's timeline, only Bella's cheeps are shown
        var bellaTimeline = cheepService.GetCheepsFromAuthor("Bella", 1);
        Assert.Equal(2, bellaTimeline.Count);
        Assert.All(bellaTimeline, c => Assert.Equal("Bella", c.Author));
        Assert.DoesNotContain(bellaTimeline, c => c.Author == "Cheryl");

        // Act & Assert - When Anna views Cheryl's timeline, only Cheryl's cheeps are shown
        var cherylTimeline = cheepService.GetCheepsFromAuthor("Cheryl", 1);
        Assert.Single(cherylTimeline);
        Assert.Equal("Cheryl", cherylTimeline[0].Author);
    }

    [Fact]
    public async Task FollowRelationship_IsDirectional()
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        using var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        var anna = new Author
        {
            UserName = "Anna",
            Email = "anna@example.com",
            Cheeps = new List<Cheep>()
        };

        var bella = new Author
        {
            UserName = "Bella",
            Email = "bella@example.com",
            Cheeps = new List<Cheep>()
        };

        context.Authors.AddRange(anna, bella);
        await context.SaveChangesAsync();

        IAuthorRepository authorRepository = new AuthorRepository(context);
        IHashtagRepository hashtagRepository = new HashtagRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, authorRepository, hashtagRepository);
        ICheepService cheepService = new CheepService(cheepRepository, authorRepository);

        // Act - Anna follows Bella (but not vice versa)
        await cheepService.FollowAuthor("Anna", "Bella");

        // Assert
        var annaFollowsBella = await cheepService.IsFollowing("Anna", "Bella");
        var bellaFollowsAnna = await cheepService.IsFollowing("Bella", "Anna");

        Assert.True(annaFollowsBella);
        Assert.False(bellaFollowsAnna);
    }
}
