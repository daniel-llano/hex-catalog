using HexCatalog.Admin.Application.Abstractions;
using HexCatalog.Admin.Application.Contracts;

namespace HexCatalog.Admin.Infrastructure.Http;

public class UserApiClient(HttpClient http, IAccessTokenProvider tokens) : ApiClientBase(http, tokens), IUserApiClient
{
    public Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
        => SendAsync<IReadOnlyList<UserDto>>(HttpMethod.Get, "api/users", null, ct);

    public Task<UserDto> ChangeRoleAsync(Guid userId, string role, CancellationToken ct = default)
        => SendAsync<UserDto>(HttpMethod.Put, $"api/users/{userId}/role", new ChangeRoleRequest(role), ct);

    public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
        => (await SendAsync(HttpMethod.Delete, $"api/users/{userId}", null, ct)).Dispose();
}
