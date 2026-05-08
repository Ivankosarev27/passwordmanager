using PasswordManager.Web.Data.Entities;
using PasswordManager.Web.Data.Repositories;

namespace PasswordManager.Web.Services;

public class TagService
{
    private readonly ITagRepository _repo;

    public TagService(ITagRepository repo) => _repo = repo;

    public Task<List<Tag>> GetAllAsync(string userId) => _repo.GetAllAsync(userId);

    public async Task<Tag> CreateAsync(string userId, string name, string color)
    {
        var tag = new Tag { Name = name.Trim(), Color = color, UserId = userId };
        return await _repo.CreateAsync(tag);
    }

    public Task<bool> DeleteAsync(int id, string userId) => _repo.DeleteAsync(id, userId);
}
