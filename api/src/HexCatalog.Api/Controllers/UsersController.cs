using HexCatalog.Api.Infrastructure;
using HexCatalog.Application.Commands.Users;
using HexCatalog.Application.DTOs;
using HexCatalog.Application.Queries.Users;
using HexCatalog.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexCatalog.Api.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize(Roles = Roles.Admin)]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<UserDto>> GetAll(CancellationToken ct)
        => mediator.Send(new GetAllUsersQuery(), ct);

    [HttpPut("{id:guid}/role")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<UserDto> ChangeRole(Guid id, ChangeRoleRequest request, CancellationToken ct)
        => mediator.Send(new ChangeUserRoleCommand(id, request.Role, User.UserId()), ct);

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteUserCommand(id, User.UserId()), ct);
        return NoContent();
    }
}
