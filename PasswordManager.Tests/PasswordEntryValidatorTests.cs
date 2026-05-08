using FluentAssertions;
using PasswordManager.Web.Data.Entities;
using PasswordManager.Web.Validators;

namespace PasswordManager.Tests;

public class PasswordEntryValidatorTests
{
    private readonly PasswordEntryValidator _validator = new();

    [Fact]
    public async Task Valid_Entry_ShouldPass()
    {
        var entry = new PasswordEntry
        {
            SiteName = "Gmail",
            Username = "user@gmail.com",
            Password = "secret123",
            SiteUrl = "https://mail.google.com"
        };

        var result = await _validator.ValidateAsync(entry);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Empty_SiteName_ShouldFail(string? siteName)
    {
        var entry = new PasswordEntry
        {
            SiteName = siteName!,
            Username = "user",
            Password = "p"
        };

        var result = await _validator.ValidateAsync(entry);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PasswordEntry.SiteName));
    }

    [Fact]
    public async Task Empty_Password_ShouldFail()
    {
        var entry = new PasswordEntry
        {
            SiteName = "Gmail",
            Username = "user",
            Password = ""
        };

        var result = await _validator.ValidateAsync(entry);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PasswordEntry.Password));
    }

    [Fact]
    public async Task Invalid_Url_ShouldFail()
    {
        var entry = new PasswordEntry
        {
            SiteName = "Site",
            Username = "u",
            Password = "p",
            SiteUrl = "not-a-valid-url"
        };

        var result = await _validator.ValidateAsync(entry);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PasswordEntry.SiteUrl));
    }

    [Fact]
    public async Task Empty_Url_ShouldPass()
    {
        var entry = new PasswordEntry
        {
            SiteName = "Site",
            Username = "u",
            Password = "p",
            SiteUrl = ""
        };

        var result = await _validator.ValidateAsync(entry);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task TooLong_SiteName_ShouldFail()
    {
        var entry = new PasswordEntry
        {
            SiteName = new string('A', 201),
            Username = "u",
            Password = "p"
        };

        var result = await _validator.ValidateAsync(entry);

        result.IsValid.Should().BeFalse();
    }
}
