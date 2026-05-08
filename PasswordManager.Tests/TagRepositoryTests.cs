using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data;
using PasswordManager.Web.Data.Entities;
using PasswordManager.Web.Data.Repositories;

namespace PasswordManager.Tests;

public class TagRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly TagRepository _repo;
    private const string UserId = "tag-user";

    public TagRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TagDb_{Guid.NewGuid()}")
            .Options;
        _db = new ApplicationDbContext(options);
        _repo = new TagRepository(_db);
    }

    [Fact]
    public async Task CreateAsync_ShouldStoreTag()
    {
        var tag = await _repo.CreateAsync(new Tag { Name = "Important", Color = "#ff0000", UserId = UserId });

        tag.Id.Should().BeGreaterThan(0);
        (await _db.Tags.FindAsync(tag.Id))!.Name.Should().Be("Important");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyUsersTags()
    {
        await _repo.CreateAsync(new Tag { Name = "Mine",  UserId = UserId });
        await _repo.CreateAsync(new Tag { Name = "Other", UserId = "another" });

        var result = await _repo.GetAllAsync(UserId);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Mine");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSorted()
    {
        await _repo.CreateAsync(new Tag { Name = "Zebra", UserId = UserId });
        await _repo.CreateAsync(new Tag { Name = "Alpha", UserId = UserId });

        var result = await _repo.GetAllAsync(UserId);

        result.Select(t => t.Name).Should().ContainInOrder("Alpha", "Zebra");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTag()
    {
        var tag = await _repo.CreateAsync(new Tag { Name = "Temp", UserId = UserId });

        var ok = await _repo.DeleteAsync(tag.Id, UserId);

        ok.Should().BeTrue();
    }

    [Fact]
    public async Task ManyToMany_PasswordEntry_Tags_WorksCorrectly()
    {
        var tag1 = await _repo.CreateAsync(new Tag { Name = "Work",   UserId = UserId });
        var tag2 = await _repo.CreateAsync(new Tag { Name = "Urgent", UserId = UserId });

        var entry = new PasswordEntry
        {
            SiteName = "GitHub", Username = "u", Password = "p", UserId = UserId,
            Tags = new List<Tag> { tag1, tag2 }
        };
        _db.PasswordEntries.Add(entry);
        await _db.SaveChangesAsync();

        var fromDb = await _db.PasswordEntries.Include(p => p.Tags).FirstAsync(p => p.Id == entry.Id);
        fromDb.Tags.Should().HaveCount(2);
        fromDb.Tags.Select(t => t.Name).Should().BeEquivalentTo(new[] { "Work", "Urgent" });

        // С другой стороны N:N — у тега тоже видна запись
        var tagWithEntries = await _db.Tags.Include(t => t.PasswordEntries).FirstAsync(t => t.Id == tag1.Id);
        tagWithEntries.PasswordEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByPasswordEntryAsync_ShouldReturnAttachedTags()
    {
        var tag = await _repo.CreateAsync(new Tag { Name = "Personal", UserId = UserId });
        var entry = new PasswordEntry
        {
            SiteName = "X", Username = "u", Password = "p", UserId = UserId,
            Tags = new List<Tag> { tag }
        };
        _db.PasswordEntries.Add(entry);
        await _db.SaveChangesAsync();

        var result = await _repo.GetByPasswordEntryAsync(entry.Id);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Personal");
    }

    public void Dispose() => _db.Dispose();
}
