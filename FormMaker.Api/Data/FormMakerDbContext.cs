using FormMaker.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormMaker.Api.Data;

/// <summary>
/// Database context for Form Maker application
/// </summary>
public class FormMakerDbContext : DbContext
{
    public FormMakerDbContext(DbContextOptions<FormMakerDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Users table
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Templates table
    /// </summary>
    public DbSet<Template> Templates { get; set; }

    /// <summary>
    /// Forms table (shareable instances)
    /// </summary>
    public DbSet<Form> Forms { get; set; }

    /// <summary>
    /// Submissions table
    /// </summary>
    public DbSet<Submission> Submissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100);

            // Configure relationship: User -> Templates (one-to-many)
            entity.HasMany(e => e.Templates)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query filter to exclude soft-deleted users
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Template entity
        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Name });
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsPublic);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.JsonData).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50);

            // Configure relationship: Template -> Forms (one-to-many)
            entity.HasMany(e => e.Forms)
                .WithOne(e => e.Template)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query filter to exclude soft-deleted templates
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Form entity
        modelBuilder.Entity<Form>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShareLink).IsUnique();
            entity.HasIndex(e => e.TemplateId);
            entity.HasIndex(e => new { e.IsActive, e.IsPublic });
            entity.Property(e => e.ShareLink).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            // Configure relationship: Form -> Submissions (one-to-many)
            entity.HasMany(e => e.Submissions)
                .WithOne(e => e.Form)
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query filter to exclude soft-deleted forms
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Submission entity
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FormId);
            entity.HasIndex(e => e.SubmittedAt);
            entity.HasIndex(e => new { e.FormId, e.SubmittedAt });
            entity.Property(e => e.JsonData).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6 max length
            entity.Property(e => e.SubmitterEmail).HasMaxLength(256);

            // Query filter to exclude soft-deleted submissions
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
