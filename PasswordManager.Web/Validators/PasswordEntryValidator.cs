using FluentValidation;
using PasswordManager.Web.Data.Entities;

namespace PasswordManager.Web.Validators;

public class PasswordEntryValidator : AbstractValidator<PasswordEntry>
{
    public PasswordEntryValidator()
    {
        RuleFor(x => x.SiteName)
            .NotEmpty().WithMessage("Название сайта обязательно")
            .MaximumLength(200).WithMessage("Не более 200 символов");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Логин или e-mail обязателен")
            .MaximumLength(200).WithMessage("Не более 200 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(1).WithMessage("Пароль не может быть пустым")
            .MaximumLength(1000).WithMessage("Не более 1000 символов");

        RuleFor(x => x.SiteUrl)
            .MaximumLength(500).WithMessage("URL не более 500 символов")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Введите корректный URL (например https://example.com)");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Заметки не более 2000 символов");
    }
}
