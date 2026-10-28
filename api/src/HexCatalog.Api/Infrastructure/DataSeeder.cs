using HexCatalog.Domain;
using HexCatalog.Domain.Entities;
using HexCatalog.Domain.Ports;

namespace HexCatalog.Api.Infrastructure;

public static class DataSeeder
{
    /// <summary>
    /// Creates the bootstrap admin when Bootstrap:AdminUsername/AdminPassword are set,
    /// and a starter catalog when SeedSampleData is true and the table is empty.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var products = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var username = config["Bootstrap:AdminUsername"];
        var password = config["Bootstrap:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password)
            && await users.GetByUsernameAsync(username, ct) is null)
        {
            await users.AddAsync(User.Create(username, BCrypt.Net.BCrypt.HashPassword(password), Roles.Admin), ct);
        }

        if (config.GetValue<bool>("SeedSampleData") && !await products.AnyAsync(ct))
        {
            await products.AddAsync(Product.Create("Hex Laptop", "14-inch ultrabook, 32 GB RAM, 1 TB SSD.", 1299.00m, "Electronics", 12), ct);
            await products.AddAsync(Product.Create("Standing Desk", "Electric sit-stand desk, 160 x 80 cm oak top.", 499.00m, "Furniture", 8), ct);
            await products.AddAsync(Product.Create("Ergonomic Chair", "Mesh back, adjustable lumbar support and armrests.", 349.00m, "Furniture", 15), ct);
            await products.AddAsync(Product.Create("4K Monitor", "27-inch IPS panel with USB-C power delivery.", 429.00m, "Electronics", 20), ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
