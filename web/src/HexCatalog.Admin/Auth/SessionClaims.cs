using System.Security.Claims;
using HexCatalog.Admin.Application.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HexCatalog.Admin.Auth;

/// <summary>
/// The admin keeps the API's access token inside its own encrypted auth cookie.
/// Interactive components read it back from the authentication state.
/// </summary>
public static class SessionClaims
{
    public const string AccessToken = "hexcatalog:access_token";
    public const string RefreshToken = "hexcatalog:refresh_token";

    public static ClaimsPrincipal CreatePrincipal(AuthResponse auth)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, auth.Username),
            new Claim(ClaimTypes.Role, auth.Role),
            new Claim(AccessToken, auth.AccessToken),
            new Claim(RefreshToken, auth.RefreshToken)
        ], CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
