using PasswordManager.Web.Data.Entities;
using PasswordManager.Web.Data.Repositories;

namespace PasswordManager.Web.Services;

public class UserSettingsService
{
    private readonly IUserSettingsRepository _repo;

    public UserSettingsService(IUserSettingsRepository repo) => _repo = repo;

    public Task<UserSettings> GetSettingsAsync(string userId) => _repo.GetOrCreateAsync(userId);

    public Task<UserSettings> UpdateSettingsAsync(UserSettings settings) => _repo.UpdateAsync(settings);
}
