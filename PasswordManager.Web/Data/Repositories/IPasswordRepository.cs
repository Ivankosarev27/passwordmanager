using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Data.Repositories;

public interface IPasswordRepository
{
    Task<List<PasswordEntry>> GetAllAsync(string userId, string? search = null, int? categoryId = null);
    Task<PasswordEntry?> GetByIdAsync(int id, string userId);
    Task<PasswordEntry> CreateAsync(PasswordEntry entry);
    Task<PasswordEntry> UpdateAsync(PasswordEntry entry);
    Task<bool> DeleteAsync(int id, string userId);
    Task<int> CountAsync(string userId);
    Task<List<PasswordEntry>> GetFavoritesAsync(string userId);
}
