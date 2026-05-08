using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Data.Repositories;

public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly ApplicationDbContext _db;

    public UserSettingsRepository(ApplicationDbContext db) => _db = db;

    public async Task<UserSettings> GetOrCreateAsync(string userId)
    {
        var existing = await _db.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (existing != null) return existing;

        var settings = new UserSettings { UserId = userId };
        _db.UserSettings.Add(settings);
        await _db.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateAsync(UserSettings settings)
    {
        _db.UserSettings.Update(settings);
        await _db.SaveChangesAsync();
        return settings;
    }
}
