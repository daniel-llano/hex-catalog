using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HexCatalog.Application.Interfaces;
using HexCatalog.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HexCatalog.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly SigningCredentials _credentials;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _lifetime;

    public JwtService(IConfiguration config)
    {
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        _credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256);
        _issuer = config["Jwt:Issuer"] ?? "HexCatalog";
        _audience = config["Jwt:Audience"] ?? "HexCatalogClients";
        _lifetime = TimeSpan.FromMinutes(config.GetValue("Jwt:AccessTokenMinutes", 60));
    }

    public AccessToken GenerateToken(User user)
    {
        var expires = DateTime.UtcNow.Add(_lifetime);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(_issuer, _audience, claims, expires: expires, signingCredentials: _credentials);
        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
