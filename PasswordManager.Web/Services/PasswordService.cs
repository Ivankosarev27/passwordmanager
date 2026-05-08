using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data;
using PasswordManager.Web.Data.Entities;
using PasswordManager.Web.Data.Repositories;

namespace PasswordManager.Web.Services;

public class PasswordService
{
    private readonly IPasswordRepository _repo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ITagRepository _tagRepo;
    private readonly ApplicationDbContext _db;

    public PasswordService(
        IPasswordRepository repo,
        ICategoryRepository categoryRepo,
        ITagRepository tagRepo,
        ApplicationDbContext db)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
        _tagRepo = tagRepo;
        _db = db;
    }

    public Task<List<PasswordEntry>> GetPasswordsAsync(string userId, string? search = null, int? categoryId = null) =>
        _repo.GetAllAsync(userId, search, categoryId);

    public Task<PasswordEntry?> GetPasswordAsync(int id, string userId) =>
        _repo.GetByIdAsync(id, userId);

    public Task<PasswordEntry> AddPasswordAsync(PasswordEntry entry) =>
        _repo.CreateAsync(entry);

    public Task<PasswordEntry> UpdatePasswordAsync(PasswordEntry entry) =>
        _repo.UpdateAsync(entry);

    public Task<bool> DeletePasswordAsync(int id, string userId) =>
        _repo.DeleteAsync(id, userId);

    public Task<int> GetTotalCountAsync(string userId) =>
        _repo.CountAsync(userId);

    public Task<List<PasswordEntry>> GetFavoritesAsync(string userId) =>
        _repo.GetFavoritesAsync(userId);

    // ── Categories ──────────────────────────────────────────────────────
    public Task<List<Category>> GetCategoriesAsync(string userId) =>
        _categoryRepo.GetAllAsync(userId);

    public Task<Category> AddCategoryAsync(Category category) =>
        _categoryRepo.CreateAsync(category);

    // ── Tags (N:N) ──────────────────────────────────────────────────────
    public Task<List<Tag>> GetTagsAsync(string userId) =>
        _tagRepo.GetAllAsync(userId);

    public Task<Tag> AddTagAsync(Tag tag) =>
        _tagRepo.CreateAsync(tag);

    public Task<bool> DeleteTagAsync(int id, string userId) =>
        _tagRepo.DeleteAsync(id, userId);

    /// <summary>Заменяет набор тегов у записи на указанный.</summary>
    public async Task SetPasswordTagsAsync(int passwordEntryId, string userId, List<int> tagIds)
    {
        var entry = await _db.PasswordEntries
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == passwordEntryId && p.UserId == userId);
        if (entry is null) return;

        entry.Tags.Clear();

        if (tagIds.Count > 0)
        {
            var tags = await _db.Tags
                .Where(t => tagIds.Contains(t.Id) && t.UserId == userId)
                .ToListAsync();
            foreach (var t in tags) entry.Tags.Add(t);
        }

        await _db.SaveChangesAsync();
    }
}
