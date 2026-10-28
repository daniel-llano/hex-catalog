namespace HexCatalog.Domain.Entities;

public class RefreshToken
{
    // SHA-256 of the token handed to the client. The raw value is never stored.
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime Expires { get; set; }
}
