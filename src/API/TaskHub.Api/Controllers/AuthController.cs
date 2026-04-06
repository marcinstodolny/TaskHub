using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Api.Auth;
using TaskHub.Application.Features.Auth.Commands;
using TaskHub.Contracts.Auth;

namespace TaskHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IMediator mediator,
    IAuthSchemaAvailabilityChecker authSchemaAvailabilityChecker,
    DatabaseUserAuthenticator userAuthenticator,
    JwtTokenGenerator tokenGenerator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegistrationResponse>> Register([FromBody] RegistrationRequest request, CancellationToken cancellationToken)
    {
        if (!await authSchemaAvailabilityChecker.IsUsersSchemaAvailableAsync(cancellationToken))
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Authentication is unavailable",
                detail: "The users schema is not available yet. Apply database migrations and try again.");
        }

        var result = await mediator.Send(new RegisterUserCommand(request.Username, request.Password, request.DisplayName), cancellationToken);

        return result.IsFailed
            ? this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to register user")
            : Ok(new RegistrationResponse(result.Value.UserId, result.Value.Username, result.Value.DisplayName, result.Value.Role));
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> CreateToken([FromBody] TokenRequest request, CancellationToken cancellationToken)
    {
        if (!await authSchemaAvailabilityChecker.IsUsersSchemaAvailableAsync(cancellationToken))
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Authentication is unavailable",
                detail: "The users schema is not available yet. Apply database migrations and try again.");
        }

        var user = await userAuthenticator.AuthenticateAsync(request.Username, request.Password, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        var token = tokenGenerator.Generate(user.Id, user.Username, user.Role);
        return Ok(new TokenResponse(token));
    }

}
