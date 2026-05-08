using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Data.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(string userId);
    Task<Category> CreateAsync(Category category);
    Task<bool> DeleteAsync(int id, string userId);
}
