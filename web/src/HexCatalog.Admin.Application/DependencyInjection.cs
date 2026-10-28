using HexCatalog.Admin.Application.Services;
using HexCatalog.Admin.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace HexCatalog.Admin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductAppService>();
        return services;
    }
}
