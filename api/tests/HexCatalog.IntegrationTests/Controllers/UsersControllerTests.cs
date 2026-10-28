using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HexCatalog.Application.DTOs;
using HexCatalog.IntegrationTests.Helpers;

namespace HexCatalog.IntegrationTests.Controllers;

public class UsersControllerTests(HexCatalogFactory factory) : IClassFixture<HexCatalogFactory>
{
    private async Task<(HttpClient Client, List<UserDto> Users)> AdminAsync()
    {
        var client = factory.CreateClient();
        client.SetBearer(await AuthHelper.AdminTokenAsync(client));
        var users = await client.GetFromJsonAsync<List<UserDto>>("/api/users");
        return (client, users!);
    }

    [Fact]
    public async Task List_AsNonAdmin_Returns403()
    {
        var client = factory.CreateClient();
        client.SetBearer(await AuthHelper.TokenForRoleAsync(client, "User"));

        (await client.GetAsync("/api/users")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangeRole_PromotesUser()
    {
        var registered = await AuthHelper.RegisterAsync(factory.CreateClient());
        var (client, users) = await AdminAsync();
        var id = users.Single(u => u.Username == registered.Username).Id;

        var response = await client.PutAsJsonAsync($"/api/users/{id}/role", new ChangeRoleRequest("User"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<UserDto>())!.Role.Should().Be("User");
    }

    [Fact]
    public async Task ChangeRole_UnknownRole_Returns400()
    {
        var registered = await AuthHelper.RegisterAsync(factory.CreateClient());
        var (client, users) = await AdminAsync();
        var id = users.Single(u => u.Username == registered.Username).Id;

        var response = await client.PutAsJsonAsync($"/api/users/{id}/role", new ChangeRoleRequest("Superuser"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Admin_CannotDemoteOrDeleteThemselves()
    {
        var (client, users) = await AdminAsync();
        var self = users.Single(u => u.Username == HexCatalogFactory.AdminUsername).Id;

        (await client.PutAsJsonAsync($"/api/users/{self}/role", new ChangeRoleRequest("Viewer")))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await client.DeleteAsync($"/api/users/{self}"))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Delete_RemovesUserAndRevokesRefreshTokens()
    {
        var registered = await AuthHelper.RegisterAsync(factory.CreateClient());
        var (client, users) = await AdminAsync();
        var id = users.Single(u => u.Username == registered.Username).Id;

        (await client.DeleteAsync($"/api/users/{id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refresh = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(registered.RefreshToken));
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
