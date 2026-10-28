using HexCatalog.Domain.Entities;

namespace HexCatalog.Application.Interfaces;

public record AccessToken(string Value, DateTime ExpiresAt);

public interface IJwtService
{
    AccessToken GenerateToken(User user);
}
