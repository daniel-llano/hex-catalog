using HexCatalog.Admin.Application.Abstractions;
using Microsoft.AspNetCore.Components.Authorization;

namespace HexCatalog.Admin.Auth;

public sealed class AccessTokenProvider(AuthenticationStateProvider authState) : IAccessTokenProvider
{
    public async Task<string?> GetAccessTokenAsync()
    {
        var state = await authState.GetAuthenticationStateAsync();
        return state.User.FindFirst(SessionClaims.AccessToken)?.Value;
    }
}
