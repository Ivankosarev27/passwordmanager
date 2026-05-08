using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data;
using PasswordManager.Web.Data.Entities;
using PasswordManager.Web.Data.Repositories;
using PasswordManager.Web.Services;

namespace PasswordManager.Tests;

public class PasswordServiceTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly PasswordService _service;
    private const string UserId = "test-user";

    public PasswordServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Db_{Guid.NewGuid()}")
            .Options;
        _db = new ApplicationDbContext(options);
        _service = new PasswordService(
            new PasswordRepository(_db),
            new CategoryRepository(_db),
            new TagRepository(_db),
            _db);
    }

    [Fact]
    public async Task AddPasswordAsync_ShouldStorePassword()
    {
        var entry = new PasswordEntry
        {
            SiteName = "GitHub",
            Username = "dev",
            Password = "ghp_xxx",
            UserId = UserId
        };

        await _service.AddPasswordAsync(entry);

        var count = await _service.GetTotalCountAsync(UserId);
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetPasswords_WithCategoryFilter_ShouldReturnFiltered()
    {
        var cat = await _service.AddCategoryAsync(new Category { Name = "Work", UserId = UserId });
        await _service.AddPasswordAsync(new PasswordEntry
        {
            SiteName = "Slack", Username = "u", Password = "p",
            UserId = UserId, CategoryId = cat.Id
        });
        await _service.AddPasswordAsync(new PasswordEntry
        {
            SiteName = "Gmail", Username = "u", Password = "p", UserId = UserId
        });

        var result = await _service.GetPasswordsAsync(UserId, categoryId: cat.Id);

        result.Should().HaveCount(1);
        result[0].SiteName.Should().Be("Slack");
    }

    [Fact]
    public async Task UpdatePassword_ShouldChangeFields()
    {
        var entry = await _service.AddPasswordAsync(new PasswordEntry
        {
            SiteName = "Site", Username = "u", Password = "old", UserId = UserId
        });

        entry.Password = "newpass";
        entry.IsFavorite = true;
        await _service.UpdatePasswordAsync(entry);

        var fetched = await _service.GetPasswordAsync(entry.Id, UserId);
        fetched!.Password.Should().Be("newpass");
        fetched.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task SetPasswordTagsAsync_ShouldAttachTags()
    {
        var entry = await _service.AddPasswordAsync(new PasswordEntry
        {
            SiteName = "X", Username = "u", Password = "p", UserId = UserId
        });
        var tag1 = await _service.AddTagAsync(new Tag { Name = "T1", UserId = UserId });
        var tag2 = await _service.AddTagAsync(new Tag { Name = "T2", UserId = UserId });

        await _service.SetPasswordTagsAsync(entry.Id, UserId, new List<int> { tag1.Id, tag2.Id });

        var fetched = await _service.GetPasswordAsync(entry.Id, UserId);
        fetched!.Tags.Should().HaveCount(2);
    }

    [Fact]
    public async Task SetPasswordTagsAsync_ShouldReplaceExistingTags()
    {
        var entry = await _service.AddPasswordAsync(new PasswordEntry
        {
            SiteName = "X", Username = "u", Password = "p", UserId = UserId
        });
        var tag1 = await _service.AddTagAsync(new Tag { Name = "T1", UserId = UserId });
        var tag2 = await _service.AddTagAsync(new Tag { Name = "T2", UserId = UserId });

        await _service.SetPasswordTagsAsync(entry.Id, UserId, new List<int> { tag1.Id });
        await _service.SetPasswordTagsAsync(entry.Id, UserId, new List<int> { tag2.Id });

        var fetched = await _service.GetPasswordAsync(entry.Id, UserId);
        fetched!.Tags.Should().HaveCount(1);
        fetched.Tags.First().Name.Should().Be("T2");
    }

    [Fact]
    public async Task DeleteTagAsync_ShouldRemoveTag()
    {
        var tag = await _service.AddTagAsync(new Tag { Name = "Tmp", UserId = UserId });

        var ok = await _service.DeleteTagAsync(tag.Id, UserId);

        ok.Should().BeTrue();
    }

    public void Dispose() => _db.Dispose();
}
