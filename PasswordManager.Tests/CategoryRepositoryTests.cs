using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data;
using PasswordManager.Web.Data.Entities;
using PasswordManager.Web.Data.Repositories;

namespace PasswordManager.Tests;

public class CategoryRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly CategoryRepository _repo;
    private const string UserId = "test-user";

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"CatDb_{Guid.NewGuid()}")
            .Options;
        _db = new ApplicationDbContext(options);
        _repo = new CategoryRepository(_db);
    }

    [Fact]
    public async Task CreateAsync_ShouldStoreCategory()
    {
        var cat = await _repo.CreateAsync(new Category { Name = "Work", Icon = "💼", UserId = UserId });

        cat.Id.Should().BeGreaterThan(0);
        var fromDb = await _db.Categories.FindAsync(cat.Id);
        fromDb!.Name.Should().Be("Work");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyOwnCategories()
    {
        await _repo.CreateAsync(new Category { Name = "Mine", Icon = "🔑", UserId = UserId });
        await _repo.CreateAsync(new Category { Name = "Other", Icon = "🔒", UserId = "other-user" });

        var result = await _repo.GetAllAsync(UserId);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Mine");
    }

    [Fact]
    public async Task DeleteAsync_OwnCategory_ShouldSucceed()
    {
        var cat = await _repo.CreateAsync(new Category { Name = "Temp", UserId = UserId });

        var ok = await _repo.DeleteAsync(cat.Id, UserId);

        ok.Should().BeTrue();
        (await _db.Categories.FindAsync(cat.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_OtherUsersCategory_ShouldFail()
    {
        var cat = await _repo.CreateAsync(new Category { Name = "Foreign", UserId = "another" });

        var ok = await _repo.DeleteAsync(cat.Id, UserId);

        ok.Should().BeFalse();
        (await _db.Categories.FindAsync(cat.Id)).Should().NotBeNull();
    }

    public void Dispose() => _db.Dispose();
}
