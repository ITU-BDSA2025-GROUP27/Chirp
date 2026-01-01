using Chirp.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class ChirpDBContext : IdentityDbContext<Author, IdentityRole<int>, int>
{
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Hashtag> Hashtags { get; set; }

    public ChirpDBContext(DbContextOptions<ChirpDBContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure many-to-many relationship for author following
        modelBuilder.Entity<Author>()
            .HasMany(a => a.Following)
            .WithMany(a => a.Followers)
            .UsingEntity(j => j.ToTable("AuthorFollows"));

        // Configure one-to-many relationship between authors and cheeps with cascade delete
        modelBuilder.Entity<Author>()
            .HasMany(a => a.Cheeps)
            .WithOne(c => c.Author)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique index on hashtag names
        modelBuilder.Entity<Hashtag>()
            .HasIndex(h => h.TagName)
            .IsUnique();

        // Configure many-to-many relationship between cheeps and hashtags
        modelBuilder.Entity<Cheep>()
            .HasMany(c => c.Hashtags)
            .WithMany(h => h.Cheeps)
            .UsingEntity(j => j.ToTable("CheepHashtags"));
    }
}
