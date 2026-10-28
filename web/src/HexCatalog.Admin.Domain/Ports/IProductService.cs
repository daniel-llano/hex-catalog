using HexCatalog.Admin.Domain.Entities;

namespace HexCatalog.Admin.Domain.Ports;

/// <summary>Product operations the admin pages depend on.</summary>
public interface IProductService
{
    Task<Page<Product>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Product> ReserveAsync(Guid id, int quantity, CancellationToken ct = default);
}
