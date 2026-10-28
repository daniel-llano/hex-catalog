using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HexCatalog.IntegrationTests.Helpers;

/// <summary>Runs the API against EF in-memory with a bootstrap admin.</summary>
public class HexCatalogFactory : WebApplicationFactory<Program>
{
    public const string AdminUsername = "testadmin";
    public const string AdminPassword = "Admin@1234";

    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("UseInMemoryDatabase", "true");
        builder.UseSetting("InMemoryDatabaseName", _dbName);
        builder.UseSetting("Jwt:Secret", "test-only-signing-key-that-is-long-enough");
        builder.UseSetting("Jwt:Issuer", "HexCatalog");
        builder.UseSetting("Jwt:Audience", "HexCatalogClients");
        builder.UseSetting("Bootstrap:AdminUsername", AdminUsername);
        builder.UseSetting("Bootstrap:AdminPassword", AdminPassword);
        builder.UseSetting("RateLimiting:AuthPermitsPerMinute", "10000");
        builder.ConfigureAppConfiguration((_, cfg) => cfg.AddInMemoryCollection());
    }
}
