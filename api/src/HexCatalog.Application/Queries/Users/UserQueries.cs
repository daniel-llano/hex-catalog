using HexCatalog.Application.Commands.Users;
using HexCatalog.Application.DTOs;
using HexCatalog.Domain.Ports;
using MediatR;

namespace HexCatalog.Application.Queries.Users;

public record GetAllUsersQuery : IRequest<IReadOnlyList<UserDto>>;

public class GetAllUsersHandler(IUserRepository users) : IRequestHandler<GetAllUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var all = await users.GetAllAsync(ct);
        return all.Select(u => u.ToDto()).ToList();
    }
}
