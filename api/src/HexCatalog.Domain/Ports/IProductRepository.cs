using HexCatalog.Domain.Entities;

namespace HexCatalog.Domain.Ports;

/// <summary>Product persistence.</summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(string? q, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
}
