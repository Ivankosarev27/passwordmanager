using Microsoft.AspNetCore.Components;

namespace PasswordManager.Web.Components.Account;

internal sealed class IdentityRedirectManager
{
    private readonly NavigationManager _navigation;

    public IdentityRedirectManager(NavigationManager navigation) => _navigation = navigation;

    public void RedirectTo(string? uri)
    {
        uri ??= "/";
        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
            uri = "/";
        _navigation.NavigateTo(uri, forceLoad: true);
    }

    public void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        context.Response.Headers["Location"] = uri;
        RedirectTo(uri);
    }
}
