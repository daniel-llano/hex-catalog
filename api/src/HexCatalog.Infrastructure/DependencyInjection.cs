using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HexCatalog.Application.Interfaces;
using HexCatalog.Domain.Ports;
using HexCatalog.Infrastructure.Data;
using HexCatalog.Infrastructure.Repositories;
using HexCatalog.Infrastructure.Services;

namespace HexCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            if (bool.TryParse(config["UseInMemoryDatabase"], out var useInMemory) && useInMemory)
            {
                options.UseInMemoryDatabase(config["InMemoryDatabaseName"] ?? "HexCatalogTests");
            }
            else
            {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            }
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenStore, EfRefreshTokenStore>();

        return services;
    }
}
