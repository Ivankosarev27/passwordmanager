using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<PasswordEntry> PasswordEntries => Set<PasswordEntry>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.DisplayName).HasMaxLength(100);
        });

        builder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(50).IsRequired();
            e.Property(c => c.Icon).HasMaxLength(10).HasDefaultValue("🔑");

            // 1:N — User → Categories
            e.HasOne(c => c.User)
             .WithMany(u => u.Categories)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PasswordEntry>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.SiteName).HasMaxLength(200).IsRequired();
            e.Property(p => p.SiteUrl).HasMaxLength(500);
            e.Property(p => p.Username).HasMaxLength(200).IsRequired();
            e.Property(p => p.Password).HasMaxLength(1000).IsRequired();
            e.Property(p => p.Notes).HasMaxLength(2000);

            // 1:N — User → PasswordEntries
            e.HasOne(p => p.User)
             .WithMany(u => u.PasswordEntries)
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // N:1 — PasswordEntry → Category (optional)
            e.HasOne(p => p.Category)
             .WithMany(c => c.PasswordEntries)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(p => p.UserId);
            e.HasIndex(p => new { p.UserId, p.SiteName });

            // N:N — PasswordEntry ↔ Tag через join-таблицу PasswordEntryTags
            e.HasMany(p => p.Tags)
             .WithMany(t => t.PasswordEntries)
             .UsingEntity<Dictionary<string, object>>(
                 "PasswordEntryTags",
                 j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                 j => j.HasOne<PasswordEntry>().WithMany().HasForeignKey("PasswordEntryId").OnDelete(DeleteBehavior.Cascade),
                 j =>
                 {
                     j.HasKey("PasswordEntryId", "TagId");
                     j.ToTable("PasswordEntryTags");
                 });
        });

        builder.Entity<Tag>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).HasMaxLength(50).IsRequired();
            e.Property(t => t.Color).HasMaxLength(20).HasDefaultValue("#6366f1");

            // 1:N — User → Tags
            e.HasOne(t => t.User)
             .WithMany(u => u.Tags)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(t => new { t.UserId, t.Name }).IsUnique();
        });

        builder.Entity<UserSettings>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Theme).HasMaxLength(20).HasDefaultValue("light");
            e.Property(s => s.Language).HasMaxLength(5).HasDefaultValue("ru");

            // 1:1 — User ↔ UserSettings (UserId уникален)
            e.HasOne(s => s.User)
             .WithOne(u => u.Settings)
             .HasForeignKey<UserSettings>(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(s => s.UserId).IsUnique();
        });

    }
}
