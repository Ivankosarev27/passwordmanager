using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Web.Components;
using PasswordManager.Web.Components.Account;
using PasswordManager.Web.Data;
using PasswordManager.Web.Data.Entities;
using PasswordManager.Web.Data.Repositories;
using PasswordManager.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Data Protection — ключи cookie сохраняются между перезапусками ──
var keysPath = builder.Configuration["DataProtection:KeysPath"] ?? "/keys";
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("PassVault");

// ── Blazor ──────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider,
    IdentityRevalidatingAuthenticationStateProvider>();

// ── Identity ─────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=postgres;Port=5432;Database=passvault;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// ── DI ───────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IPasswordRepository, PasswordRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<UserSettingsService>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── App ──────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// Apply migrations on startup with retry (PostgreSQL может ещё подниматься)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var defined = db.Database.GetMigrations().ToList();
    logger.LogInformation("Migrations defined in assembly: {Count} ({Names})",
        defined.Count, string.Join(", ", defined));

    if (defined.Count == 0)
    {
        logger.LogError("No migrations were discovered. Check that migration classes have [Migration] attribute.");
    }

    const int maxAttempts = 10;
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            db.Database.Migrate();
            var applied = db.Database.GetAppliedMigrations().ToList();
            logger.LogInformation("Migrations applied: {Count} ({Names})",
                applied.Count, string.Join(", ", applied));
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning("Database not ready (attempt {Attempt}/{Max}): {Message}",
                attempt, maxAttempts, ex.Message);
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
    }
}

app.Run();
