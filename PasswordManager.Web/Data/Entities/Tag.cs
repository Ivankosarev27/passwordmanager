namespace PasswordManager.Web.Data.Entities;

/// <summary>
/// Тег. N:N с PasswordEntry — пароль может иметь много тегов, тег применим к многим паролям.
/// </summary>
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    // N:N
    public ICollection<PasswordEntry> PasswordEntries { get; set; } = new List<PasswordEntry>();
}
