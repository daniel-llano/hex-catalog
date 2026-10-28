using HexCatalog.Application.DTOs;
using HexCatalog.Application.Interfaces;
using HexCatalog.Domain.Entities;
using HexCatalog.Domain.Exceptions;
using HexCatalog.Domain.Ports;
using MediatR;

namespace HexCatalog.Application.Commands.Users;

public record ChangeUserRoleCommand(Guid UserId, string Role, Guid ActorId) : IRequest<UserDto>;

public class ChangeUserRoleHandler(IUserRepository users, IUnitOfWork uow) : IRequestHandler<ChangeUserRoleCommand, UserDto>
{
    public async Task<UserDto> Handle(ChangeUserRoleCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == cmd.ActorId)
            throw new BusinessRuleException("You can't change your own role.");

        var user = await users.GetByIdAsync(cmd.UserId, ct)
            ?? throw new NotFoundException(nameof(User), cmd.UserId);

        user.ChangeRole(cmd.Role);
        await uow.SaveChangesAsync(ct);
        return user.ToDto();
    }
}

public record DeleteUserCommand(Guid UserId, Guid ActorId) : IRequest;

public class DeleteUserHandler(IUserRepository users, IRefreshTokenStore refresh, IUnitOfWork uow) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == cmd.ActorId)
            throw new BusinessRuleException("You can't delete your own account.");

        var user = await users.GetByIdAsync(cmd.UserId, ct)
            ?? throw new NotFoundException(nameof(User), cmd.UserId);

        await users.DeleteAsync(user, ct);
        await refresh.RevokeAllAsync(user.Id, ct);
        await uow.SaveChangesAsync(ct);
    }
}

internal static class UserMappings
{
    internal static UserDto ToDto(this User u) => new(u.Id, u.Username, u.Role, u.CreatedAt);
}
