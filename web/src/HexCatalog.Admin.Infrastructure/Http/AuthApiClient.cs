using System.Net;
using HexCatalog.Admin.Application.Abstractions;
using HexCatalog.Admin.Application.Contracts;

namespace HexCatalog.Admin.Infrastructure.Http;

public class AuthApiClient(HttpClient http) : ApiClientBase(http, tokens: null), IAuthApiClient
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        try
        {
            return await SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/login", request, ct);
        }
        catch (ApiException ex) when (ex.Status == HttpStatusCode.Unauthorized)
        {
            return null;
        }
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/register", request, ct);

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        try
        {
            return await SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/refresh", new RefreshRequest(refreshToken), ct);
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            return null;
        }
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
        => (await SendAsync(HttpMethod.Post, "api/auth/logout", new RefreshRequest(refreshToken), ct)).Dispose();
}
