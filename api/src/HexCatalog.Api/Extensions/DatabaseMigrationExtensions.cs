using Microsoft.EntityFrameworkCore;
using HexCatalog.Infrastructure.Data;

namespace HexCatalog.Api.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsIfConfiguredAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
            return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
