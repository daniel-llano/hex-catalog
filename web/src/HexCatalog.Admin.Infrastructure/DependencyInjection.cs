using HexCatalog.Admin.Application.Abstractions;
using HexCatalog.Admin.Infrastructure.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexCatalog.Admin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var baseUrl = config["ApiSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("ApiSettings:BaseUrl is required.");
        var apiBase = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");

        void Configure(HttpClient client)
        {
            client.BaseAddress = apiBase;
            client.Timeout = TimeSpan.FromSeconds(30);
        }

        services.AddHttpClient<IAuthApiClient, AuthApiClient>(Configure);
        services.AddHttpClient<IProductApiClient, ProductApiClient>(Configure);
        services.AddHttpClient<IUserApiClient, UserApiClient>(Configure);
        return services;
    }
}
