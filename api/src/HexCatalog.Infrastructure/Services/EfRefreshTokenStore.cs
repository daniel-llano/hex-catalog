using System.Security.Cryptography;
using System.Text;
using HexCatalog.Application.Interfaces;
using HexCatalog.Domain.Entities;
using HexCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HexCatalog.Infrastructure.Services;

public sealed class EfRefreshTokenStore(AppDbContext db, IConfiguration config) : IRefreshTokenStore
{
    private readonly TimeSpan _lifetime = TimeSpan.FromDays(config.GetValue("Jwt:RefreshTokenDays", 7));

    public async Task<string> IssueAsync(Guid userId, CancellationToken ct = default)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        db.RefreshTokens.Add(new RefreshToken
        {
            Token = Hash(token),
            UserId = userId,
            Expires = DateTime.UtcNow.Add(_lifetime)
        });
        await db.SaveChangesAsync(ct);
        return token;
    }

    public async Task<Guid?> RedeemAsync(string token, CancellationToken ct = default)
    {
        var hash = Hash(token);
        var row = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == hash, ct);
        if (row is null) return null;

        db.RefreshTokens.Remove(row);
        await db.SaveChangesAsync(ct);
        return row.Expires < DateTime.UtcNow ? null : row.UserId;
    }

    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        var hash = Hash(token);
        var row = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == hash, ct);
        if (row is null) return;
        db.RefreshTokens.Remove(row);
        await db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllAsync(Guid userId, CancellationToken ct = default)
    {
        var rows = await db.RefreshTokens.Where(t => t.UserId == userId).ToListAsync(ct);
        db.RefreshTokens.RemoveRange(rows);
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
