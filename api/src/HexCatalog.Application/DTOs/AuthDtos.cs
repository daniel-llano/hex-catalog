namespace HexCatalog.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(string Username, string Password);

public record AuthResponse(string AccessToken, string Username, string Role, DateTime ExpiresAt, string RefreshToken);

public record RefreshRequest(string RefreshToken);

public record UserDto(Guid Id, string Username, string Role, DateTime CreatedAt);

public record ChangeRoleRequest(string Role);
