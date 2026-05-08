using Microsoft.AspNetCore.Identity;

namespace PasswordManager.Web.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PasswordEntry> PasswordEntries { get; set; } = new List<PasswordEntry>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    // 1:1 — настройки пользователя
    public UserSettings? Settings { get; set; }
}
