namespace PasswordManager.Web.Data.Entities;

/// <summary>
/// 1:1 — каждый пользователь имеет один блок настроек.
/// </summary>
public class UserSettings
{
    public int Id { get; set; }

    public string Theme { get; set; } = "light";          // light | dark
    public string Language { get; set; } = "ru";          // ru | en
    public bool ShowPasswordsByDefault { get; set; }
    public int AutoLockMinutes { get; set; } = 15;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 1:1 navigation — UserId уникален
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
