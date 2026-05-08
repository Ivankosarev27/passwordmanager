using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Category>> GetAllAsync(string userId) =>
        await _db.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<Category> CreateAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (cat is null) return false;

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return true;
    }
}
