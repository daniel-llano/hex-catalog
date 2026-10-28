using HexCatalog.Domain.Entities;
using HexCatalog.Domain.Ports;
using HexCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HexCatalog.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(string? q, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Category.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default)
        => await db.Products.AsNoTracking().Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync(ct);

    public Task<bool> AnyAsync(CancellationToken ct = default) => db.Products.AnyAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default)
        => await db.Products.AddAsync(product, ct);
}
