using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data;
using PasswordManager.Web.Data.Entities;
using PasswordManager.Web.Data.Repositories;

namespace PasswordManager.Tests;

public class PasswordRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly PasswordRepository _repo;
    private const string UserId = "test-user-1";

    public PasswordRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"PasswordDb_{Guid.NewGuid()}")
            .Options;
        _db = new ApplicationDbContext(options);
        _repo = new PasswordRepository(_db);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistEntry()
    {
        var entry = NewEntry("Gmail", "user@gmail.com", "secret123");

        var created = await _repo.CreateAsync(entry);

        created.Id.Should().BeGreaterThan(0);
        var fromDb = await _db.PasswordEntries.FindAsync(created.Id);
        fromDb.Should().NotBeNull();
        fromDb!.SiteName.Should().Be("Gmail");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyUsersEntries()
    {
        await _repo.CreateAsync(NewEntry("Gmail", "u1@x", "p1", UserId));
        await _repo.CreateAsync(NewEntry("VK",    "u2@x", "p2", "other-user"));

        var result = await _repo.GetAllAsync(UserId);

        result.Should().HaveCount(1);
        result[0].SiteName.Should().Be("Gmail");
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_ShouldFilterBySiteName()
    {
        await _repo.CreateAsync(NewEntry("Gmail", "u@x", "p", UserId));
        await _repo.CreateAsync(NewEntry("ВКонтакте", "u@x", "p", UserId));

        var result = await _repo.GetAllAsync(UserId, search: "mail");

        result.Should().HaveCount(1);
        result[0].SiteName.Should().Be("Gmail");
    }

    [Fact]
    public async Task UpdateAsync_ShouldChangeUpdatedAt()
    {
        var entry = await _repo.CreateAsync(NewEntry("Gmail", "u@x", "old"));
        var originalDate = entry.UpdatedAt;
        await Task.Delay(20);

        entry.Password = "new-password";
        var updated = await _repo.UpdateAsync(entry);

        updated.Password.Should().Be("new-password");
        updated.UpdatedAt.Should().BeAfter(originalDate);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntry()
    {
        var entry = await _repo.CreateAsync(NewEntry("Gmail", "u@x", "p"));

        var result = await _repo.DeleteAsync(entry.Id, UserId);

        result.Should().BeTrue();
        (await _db.PasswordEntries.FindAsync(entry.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_OtherUsersEntry_ShouldFail()
    {
        var entry = await _repo.CreateAsync(NewEntry("Gmail", "u@x", "p", "another-user"));

        var result = await _repo.DeleteAsync(entry.Id, UserId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectNumber()
    {
        await _repo.CreateAsync(NewEntry("A", "a@x", "p"));
        await _repo.CreateAsync(NewEntry("B", "b@x", "p"));
        await _repo.CreateAsync(NewEntry("C", "c@x", "p", "other"));

        var count = await _repo.CountAsync(UserId);

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetFavoritesAsync_ShouldReturnOnlyFavorites()
    {
        var fav = NewEntry("Fav", "u@x", "p");
        fav.IsFavorite = true;
        await _repo.CreateAsync(fav);
        await _repo.CreateAsync(NewEntry("NotFav", "u@x", "p"));

        var result = await _repo.GetFavoritesAsync(UserId);

        result.Should().HaveCount(1);
        result[0].SiteName.Should().Be("Fav");
    }

    private static PasswordEntry NewEntry(string site, string user, string pwd, string userId = UserId) => new()
    {
        SiteName = site,
        Username = user,
        Password = pwd,
        UserId = userId,
        SiteUrl = "",
        Notes = ""
    };

    public void Dispose() => _db.Dispose();
}
