using System.Net;
using HexCatalog.Admin.Application.Abstractions;
using HexCatalog.Admin.Application.Contracts;

namespace HexCatalog.Admin.Infrastructure.Http;

public class ProductApiClient(HttpClient http, IAccessTokenProvider tokens) : ApiClientBase(http, tokens), IProductApiClient
{
    public Task<PagedResult<ProductDto>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default)
    {
        var q = string.IsNullOrWhiteSpace(query) ? "" : $"&q={Uri.EscapeDataString(query.Trim())}";
        return SendAsync<PagedResult<ProductDto>>(HttpMethod.Get, $"api/products?page={page}&pageSize={pageSize}{q}", null, ct);
    }

    public Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default)
        => SendAsync<IReadOnlyList<string>>(HttpMethod.Get, "api/products/categories", null, ct);

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            return await SendAsync<ProductDto>(HttpMethod.Get, $"api/products/{id}", null, ct);
        }
        catch (ApiException ex) when (ex.Status == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken ct = default)
        => SendAsync<ProductDto>(HttpMethod.Post, "api/products", request, ct);

    public Task<ProductDto> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct = default)
        => SendAsync<ProductDto>(HttpMethod.Put, $"api/products/{id}", request, ct);

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => (await SendAsync(HttpMethod.Delete, $"api/products/{id}", null, ct)).Dispose();

    public Task<ProductDto> ReserveAsync(Guid id, int quantity, CancellationToken ct = default)
        => SendAsync<ProductDto>(HttpMethod.Post, $"api/products/{id}/reserve", new ReserveStockRequest(quantity), ct);
}
