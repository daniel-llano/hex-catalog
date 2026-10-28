using HexCatalog.Application.Commands.Auth;
using HexCatalog.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HexCatalog.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[AllowAnonymous]
[EnableRateLimiting("auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>Create an account. New accounts get the Viewer role until an admin promotes them.</summary>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var response = await mediator.Send(new RegisterCommand(request.Username, request.Password), ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Sign in and get an access token plus a refresh token.</summary>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<AuthResponse> Login(LoginRequest request, CancellationToken ct)
        => await mediator.Send(new LoginCommand(request.Username, request.Password), ct);

    /// <summary>Trade a refresh token for a new pair. The old refresh token stops working.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<AuthResponse> Refresh(RefreshRequest request, CancellationToken ct)
        => await mediator.Send(new RefreshCommand(request.RefreshToken), ct);

    /// <summary>Revoke a refresh token.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct)
    {
        await mediator.Send(new LogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }
}
