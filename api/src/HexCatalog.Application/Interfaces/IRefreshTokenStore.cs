namespace HexCatalog.Application.Interfaces;

public interface IRefreshTokenStore
{
    Task<string> IssueAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Consumes the token. Returns the owner, or null when it's unknown or expired.</summary>
    Task<Guid?> RedeemAsync(string token, CancellationToken ct = default);

    Task RevokeAsync(string token, CancellationToken ct = default);
    Task RevokeAllAsync(Guid userId, CancellationToken ct = default);
}
