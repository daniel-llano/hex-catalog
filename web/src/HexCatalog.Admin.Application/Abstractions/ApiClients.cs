using HexCatalog.Admin.Application.Contracts;

namespace HexCatalog.Admin.Application.Abstractions;

public interface IAccessTokenProvider
{
    Task<string?> GetAccessTokenAsync();
}

public interface IAuthApiClient
{
    /// <summary>Returns null when the credentials are wrong.</summary>
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
}

public interface IProductApiClient
{
    Task<PagedResult<ProductDto>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> ReserveAsync(Guid id, int quantity, CancellationToken ct = default);
}

public interface IUserApiClient
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto> ChangeRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, CancellationToken ct = default);
}
