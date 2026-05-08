using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Data.Repositories;

public interface ITagRepository
{
    Task<List<Tag>> GetAllAsync(string userId);
    Task<Tag?> GetByIdAsync(int id, string userId);
    Task<Tag> CreateAsync(Tag tag);
    Task<Tag> UpdateAsync(Tag tag);
    Task<bool> DeleteAsync(int id, string userId);
    Task<List<Tag>> GetByPasswordEntryAsync(int passwordEntryId);
}
