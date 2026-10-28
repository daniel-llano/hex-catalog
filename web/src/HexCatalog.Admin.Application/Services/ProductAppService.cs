using HexCatalog.Admin.Application.Abstractions;
using HexCatalog.Admin.Application.Contracts;
using HexCatalog.Admin.Domain.Entities;
using HexCatalog.Admin.Domain.Ports;

namespace HexCatalog.Admin.Application.Services;

/// <summary>Maps between the API contracts and the admin's product model.</summary>
public class ProductAppService(IProductApiClient client) : IProductService
{
    public async Task<Page<Product>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await client.SearchAsync(query, page, pageSize, ct);
        return new Page<Product>(result.Items.Select(ToProduct).ToList(), result.Total, result.Page, result.PageSize);
    }

    public Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default) => client.GetCategoriesAsync(ct);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dto = await client.GetByIdAsync(id, ct);
        return dto is null ? null : ToProduct(dto);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
        => ToProduct(await client.CreateAsync(ToRequest(product), ct));

    public async Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
        => ToProduct(await client.UpdateAsync(product.Id, ToRequest(product), ct));

    public Task DeleteAsync(Guid id, CancellationToken ct = default) => client.DeleteAsync(id, ct);

    public async Task<Product> ReserveAsync(Guid id, int quantity, CancellationToken ct = default)
        => ToProduct(await client.ReserveAsync(id, quantity, ct));

    private static ProductRequest ToRequest(Product p) => new(p.Name, p.Description, p.Price, p.Category, p.Stock);

    private static Product ToProduct(ProductDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        Category = dto.Category,
        Stock = dto.Stock,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt
    };
}
