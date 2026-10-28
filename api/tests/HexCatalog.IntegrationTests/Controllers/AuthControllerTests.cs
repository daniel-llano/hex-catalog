using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HexCatalog.Application.DTOs;
using HexCatalog.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HexCatalog.IntegrationTests.Controllers;

public class AuthControllerTests(HexCatalogFactory factory) : IClassFixture<HexCatalogFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_NewUser_Returns201AsViewer()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("newuser", "P@ssw0rd1"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Username.Should().Be("newuser");
        body.Role.Should().Be("Viewer");
    }

    [Fact]
    public async Task Register_CannotPickAdminRole()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { username = "sneaky", password = "P@ssw0rd1", role = "Admin" });

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Role.Should().Be("Viewer");
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns409()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("dupuser", "P@ssw0rd1"));

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("dupuser", "P@ssw0rd2"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400WithFieldErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("weakling", "short"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("password");
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        await AuthHelper.RegisterAsync(_client, "loginuser");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("loginuser", "P@ssw0rd1"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401Problem()
    {
        await AuthHelper.RegisterAsync(_client, "wrongpw_user");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("wrongpw_user", "Wrong!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Login_UnknownUser_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody", "nobody"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_RotatesTokens_AndOldTokenStopsWorking()
    {
        var issued = await AuthHelper.RegisterAsync(_client);

        var first = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(issued.RefreshToken));
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await first.Content.ReadFromJsonAsync<AuthResponse>();
        body!.RefreshToken.Should().NotBe(issued.RefreshToken);

        var replay = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(issued.RefreshToken));
        replay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var issued = await AuthHelper.RegisterAsync(_client);

        var logout = await _client.PostAsJsonAsync("/api/auth/logout", new RefreshRequest(issued.RefreshToken));
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(issued.RefreshToken));
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_Returns200(string path)
    {
        var response = await _client.GetAsync(path);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
