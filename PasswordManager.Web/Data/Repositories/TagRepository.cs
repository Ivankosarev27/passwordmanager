using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Data.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ApplicationDbContext _db;

    public TagRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Tag>> GetAllAsync(string userId) =>
        await _db.Tags
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();

    public async Task<Tag?> GetByIdAsync(int id, string userId) =>
        await _db.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public async Task<Tag> CreateAsync(Tag tag)
    {
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag> UpdateAsync(Tag tag)
    {
        _db.Tags.Update(tag);
        await _db.SaveChangesAsync();
        return tag;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (tag is null) return false;
        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Tag>> GetByPasswordEntryAsync(int passwordEntryId) =>
        await _db.PasswordEntries
            .Where(p => p.Id == passwordEntryId)
            .SelectMany(p => p.Tags)
            .ToListAsync();
}
