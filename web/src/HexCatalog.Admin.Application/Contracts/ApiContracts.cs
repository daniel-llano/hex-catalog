namespace HexCatalog.Admin.Application.Contracts;

public record ProductDto(Guid Id, string Name, string Description, decimal Price, string Category, int Stock, DateTime CreatedAt, DateTime? UpdatedAt);

public record ProductRequest(string Name, string Description, decimal Price, string Category, int Stock);

public record ReserveStockRequest(int Quantity);

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public record LoginRequest(string Username, string Password);

public record RegisterRequest(string Username, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string Username, string Role, DateTime ExpiresAt, string RefreshToken);

public record UserDto(Guid Id, string Username, string Role, DateTime CreatedAt);

public record ChangeRoleRequest(string Role);
