using HexCatalog.Application.DTOs;
using HexCatalog.Application.Interfaces;
using HexCatalog.Domain;
using HexCatalog.Domain.Entities;
using HexCatalog.Domain.Exceptions;
using HexCatalog.Domain.Ports;
using MediatR;

namespace HexCatalog.Application.Commands.Auth;

public record RegisterCommand(string Username, string Password) : IRequest<AuthResponse>;

public class RegisterHandler(IUserRepository users, IUnitOfWork uow, IJwtService jwt, IRefreshTokenStore refresh)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        var username = cmd.Username.Trim();
        if (await users.GetByUsernameAsync(username, ct) is not null)
            throw new ConflictException($"Username '{username}' is already taken.");

        // Callers never pick their own role. An admin promotes the account later.
        var user = User.Create(username, BCrypt.Net.BCrypt.HashPassword(cmd.Password), Roles.Viewer);
        await users.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);

        return await Tokens.IssueAsync(jwt, refresh, user, ct);
    }
}

public record LoginCommand(string Username, string Password) : IRequest<AuthResponse>;

public class LoginHandler(IUserRepository users, IJwtService jwt, IRefreshTokenStore refresh)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await users.GetByUsernameAsync(cmd.Username.Trim(), ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        return await Tokens.IssueAsync(jwt, refresh, user, ct);
    }
}

public record RefreshCommand(string RefreshToken) : IRequest<AuthResponse>;

public class RefreshHandler(IUserRepository users, IJwtService jwt, IRefreshTokenStore refresh)
    : IRequestHandler<RefreshCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshCommand cmd, CancellationToken ct)
    {
        var userId = await refresh.RedeemAsync(cmd.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        var user = await users.GetByIdAsync(userId, ct)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        return await Tokens.IssueAsync(jwt, refresh, user, ct);
    }
}

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutHandler(IRefreshTokenStore refresh) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand cmd, CancellationToken ct) => refresh.RevokeAsync(cmd.RefreshToken, ct);
}

internal static class Tokens
{
    public static async Task<AuthResponse> IssueAsync(IJwtService jwt, IRefreshTokenStore store, User user, CancellationToken ct)
    {
        var access = jwt.GenerateToken(user);
        var refresh = await store.IssueAsync(user.Id, ct);
        return new AuthResponse(access.Value, user.Username, user.Role, access.ExpiresAt, refresh);
    }
}
