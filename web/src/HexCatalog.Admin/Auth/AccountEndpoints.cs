using HexCatalog.Admin.Application.Abstractions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HexCatalog.Admin.Auth;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/account/logout", async (HttpContext http, IAntiforgery antiforgery, IAuthApiClient auth, ILoggerFactory logs) =>
        {
            await antiforgery.ValidateRequestAsync(http);

            var refresh = http.User.FindFirst(SessionClaims.RefreshToken)?.Value;
            if (!string.IsNullOrEmpty(refresh))
            {
                try
                {
                    await auth.LogoutAsync(refresh, http.RequestAborted);
                }
                catch (Exception ex) when (ex is ApiException or HttpRequestException)
                {
                    // The local session still ends. The refresh token just expires on its own.
                    logs.CreateLogger("Account").LogWarning(ex, "Couldn't revoke the refresh token");
                }
            }

            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.LocalRedirect("~/account/login");
        }).DisableAntiforgery().RequireAuthorization();
    }
}
