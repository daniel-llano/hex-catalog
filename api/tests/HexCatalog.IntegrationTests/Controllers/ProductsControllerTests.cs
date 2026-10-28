using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HexCatalog.Application.DTOs;
using HexCatalog.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HexCatalog.IntegrationTests.Controllers;

public class ProductsControllerTests(HexCatalogFactory factory) : IClassFixture<HexCatalogFactory>
{
    private static CreateProductRequest Sample(string name = "Laptop Pro", int stock = 50)
        => new(name, "High-end laptop", 1299.99m, "Electronics", stock);

    private async Task<HttpClient> AdminClientAsync()
    {
        var client = factory.CreateClient();
        client.SetBearer(await AuthHelper.AdminTokenAsync(client));
        return client;
    }

    private async Task<ProductDto> CreateAsync(HttpClient client, CreateProductRequest request)
    {
        var resp = await client.PostAsJsonAsync("/api/products", request);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<ProductDto>())!;
    }

    [Fact]
    public async Task Create_AsAdmin_Returns201WithLocation()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/products", Sample());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var body = await response.Content.ReadFromJsonAsync<ProductDto>();
        body!.Name.Should().Be("Laptop Pro");
    }

    [Fact]
    public async Task Create_InvalidPayload_Returns400WithFieldErrors()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductRequest("", "", -5m, "", -1));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Keys.Should().Contain(["name", "description", "price", "category", "stock"]);
    }

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/products", Sample("Ghost"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsUser_Returns403()
    {
        var client = factory.CreateClient();
        client.SetBearer(await AuthHelper.TokenForRoleAsync(client, "User"));

        var response = await client.PostAsJsonAsync("/api/products", Sample("Nope"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Search_AsViewer_Returns403()
    {
        var client = factory.CreateClient();
        client.SetBearer((await AuthHelper.RegisterAsync(client)).AccessToken);

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Search_AsUser_ReturnsPagedResult()
    {
        var admin = await AdminClientAsync();
        var tag = Guid.NewGuid().ToString("N")[..6];
        for (var i = 0; i < 3; i++)
            await CreateAsync(admin, Sample($"Paged {tag} {i}"));

        var client = factory.CreateClient();
        client.SetBearer(await AuthHelper.TokenForRoleAsync(client, "User"));
        var page = await client.GetFromJsonAsync<PagedResult<ProductDto>>($"/api/products?q={tag}&page=2&pageSize=2");

        page!.Total.Should().Be(3);
        page.Page.Should().Be(2);
        page.TotalPages.Should().Be(2);
        page.Items.Should().ContainSingle().Which.Name.Should().Be($"Paged {tag} 2");
    }

    [Fact]
    public async Task Categories_ListsDistinctValues()
    {
        var client = await AdminClientAsync();
        await CreateAsync(client, Sample() with { Category = "Garden" });
        await CreateAsync(client, Sample() with { Category = "Garden" });

        var categories = await client.GetFromJsonAsync<List<string>>("/api/products/categories");

        categories.Should().ContainSingle(c => c == "Garden");
    }

    [Fact]
    public async Task GetById_Missing_Returns404Problem()
    {
        var client = await AdminClientAsync();

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Update_AsAdmin_Returns200WithUpdatedData()
    {
        var client = await AdminClientAsync();
        var product = await CreateAsync(client, Sample("OldName"));

        var response = await client.PutAsJsonAsync($"/api/products/{product.Id}",
            new UpdateProductRequest("NewName", "Updated desc", 1599.99m, "Electronics", 30));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductDto>();
        body!.Name.Should().Be("NewName");
        body.Price.Should().Be(1599.99m);
        body.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_Missing_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}",
            new UpdateProductRequest("X", "Y", 1m, "Z", 0));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ThenGet_Returns404()
    {
        var client = await AdminClientAsync();
        var product = await CreateAsync(client, Sample("DeleteMe"));

        (await client.DeleteAsync($"/api/products/{product.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/products/{product.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reserve_AsUser_DecrementsStock()
    {
        var admin = await AdminClientAsync();
        var product = await CreateAsync(admin, Sample("Reservable", stock: 5));

        var client = factory.CreateClient();
        client.SetBearer(await AuthHelper.TokenForRoleAsync(client, "User"));
        var response = await client.PostAsJsonAsync($"/api/products/{product.Id}/reserve", new ReserveStockRequest(2));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<ProductDto>())!.Stock.Should().Be(3);
    }

    [Fact]
    public async Task Reserve_MoreThanStock_Returns422()
    {
        var client = await AdminClientAsync();
        var product = await CreateAsync(client, Sample("Scarce", stock: 1));

        var response = await client.PostAsJsonAsync($"/api/products/{product.Id}/reserve", new ReserveStockRequest(2));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Reserve_ZeroQuantity_Returns400()
    {
        var client = await AdminClientAsync();
        var product = await CreateAsync(client, Sample("ZeroQty"));

        var response = await client.PostAsJsonAsync($"/api/products/{product.Id}/reserve", new ReserveStockRequest(0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
