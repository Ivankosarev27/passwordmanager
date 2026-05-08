using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Data.Repositories;

public class PasswordRepository : IPasswordRepository
{
    private readonly ApplicationDbContext _db;

    public PasswordRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<PasswordEntry>> GetAllAsync(string userId, string? search = null, int? categoryId = null)
    {
        var query = _db.PasswordEntries
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.SiteName.Contains(search) ||
                p.Username.Contains(search) ||
                p.SiteUrl.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        return await query.OrderByDescending(p => p.IsFavorite)
                          .ThenBy(p => p.SiteName)
                          .ToListAsync();
    }

    public async Task<PasswordEntry?> GetByIdAsync(int id, string userId) =>
        await _db.PasswordEntries
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

    public async Task<PasswordEntry> CreateAsync(PasswordEntry entry)
    {
        _db.PasswordEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<PasswordEntry> UpdateAsync(PasswordEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        _db.PasswordEntries.Update(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var entry = await _db.PasswordEntries
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (entry is null) return false;

        _db.PasswordEntries.Remove(entry);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAsync(string userId) =>
        await _db.PasswordEntries.CountAsync(p => p.UserId == userId);

    public async Task<List<PasswordEntry>> GetFavoritesAsync(string userId) =>
        await _db.PasswordEntries
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .Where(p => p.UserId == userId && p.IsFavorite)
            .OrderBy(p => p.SiteName)
            .ToListAsync();
}
