using System.Net.Http.Headers;
using System.Net.Http.Json;
using HexCatalog.Application.DTOs;

namespace HexCatalog.IntegrationTests.Helpers;

public static class AuthHelper
{
    public static async Task<AuthResponse> LoginAsync(HttpClient client, string username, string password)
    {
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password));
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public static async Task<string> AdminTokenAsync(HttpClient client)
        => (await LoginAsync(client, HexCatalogFactory.AdminUsername, HexCatalogFactory.AdminPassword)).AccessToken;

    public static async Task<AuthResponse> RegisterAsync(HttpClient client, string? username = null, string password = "P@ssw0rd1")
    {
        username ??= "user_" + Guid.NewGuid().ToString("N")[..8];
        var resp = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(username, password));
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    /// <summary>Registers a user and has the admin promote it to <paramref name="role"/>.</summary>
    public static async Task<string> TokenForRoleAsync(HttpClient client, string role)
    {
        var password = "P@ssw0rd1";
        var registered = await RegisterAsync(client, password: password);

        var users = await AsAdmin(client, c => c.GetFromJsonAsync<List<UserDto>>("/api/users"));
        var id = users!.Single(u => u.Username == registered.Username).Id;
        await AsAdmin(client, c => c.PutAsJsonAsync($"/api/users/{id}/role", new ChangeRoleRequest(role)));

        return (await LoginAsync(client, registered.Username, password)).AccessToken;
    }

    private static async Task<T> AsAdmin<T>(HttpClient client, Func<HttpClient, Task<T>> call)
    {
        var previous = client.DefaultRequestHeaders.Authorization;
        client.SetBearer(await AdminTokenAsync(client));
        try { return await call(client); }
        finally { client.DefaultRequestHeaders.Authorization = previous; }
    }

    public static void SetBearer(this HttpClient client, string token)
        => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
