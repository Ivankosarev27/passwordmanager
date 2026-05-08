namespace PasswordManager.Web.Data.Entities;

public class PasswordEntry
{
    public int Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string SiteUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // N:N — теги
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
