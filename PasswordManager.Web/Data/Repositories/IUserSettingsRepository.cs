using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Data.Repositories;

public interface IUserSettingsRepository
{
    Task<UserSettings> GetOrCreateAsync(string userId);
    Task<UserSettings> UpdateAsync(UserSettings settings);
}
