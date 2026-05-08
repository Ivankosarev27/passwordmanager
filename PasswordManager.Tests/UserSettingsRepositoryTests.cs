using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data;
using PasswordManager.Web.Data.Repositories;

namespace PasswordManager.Tests;

public class UserSettingsRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly UserSettingsRepository _repo;
    private const string UserId = "settings-user";

    public UserSettingsRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"SettingsDb_{Guid.NewGuid()}")
            .Options;
        _db = new ApplicationDbContext(options);
        _repo = new UserSettingsRepository(_db);
    }

    [Fact]
    public async Task GetOrCreateAsync_NewUser_ShouldCreateDefaultSettings()
    {
        var settings = await _repo.GetOrCreateAsync(UserId);

        settings.Should().NotBeNull();
        settings.Theme.Should().Be("light");
        settings.Language.Should().Be("ru");
        settings.AutoLockMinutes.Should().Be(15);
    }

    [Fact]
    public async Task GetOrCreateAsync_ExistingUser_ShouldReturnSameSettings()
    {
        var first = await _repo.GetOrCreateAsync(UserId);
        var second = await _repo.GetOrCreateAsync(UserId);

        // 1:1 — никогда не должно создаваться второй раз
        first.Id.Should().Be(second.Id);
        (await _db.UserSettings.CountAsync(s => s.UserId == UserId)).Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var settings = await _repo.GetOrCreateAsync(UserId);
        settings.Theme = "dark";
        settings.AutoLockMinutes = 30;

        await _repo.UpdateAsync(settings);

        var fromDb = await _db.UserSettings.FirstAsync(s => s.UserId == UserId);
        fromDb.Theme.Should().Be("dark");
        fromDb.AutoLockMinutes.Should().Be(30);
    }

    public void Dispose() => _db.Dispose();
}
