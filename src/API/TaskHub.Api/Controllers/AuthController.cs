using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Api.Filters;
using TaskHub.Application.Features.Auth;
using TaskHub.Application.Features.Auth.Commands;
using TaskHub.Contracts.Auth;
using ISender = MediatR.ISender;

namespace TaskHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(RequireAuthSchemaAvailableFilter))]
public sealed class AuthController(
    ISender sender) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegistrationResponse>> Register([FromBody] RegistrationRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegisterUserCommand(request.Username, request.Password, request.DisplayName), cancellationToken);

        return result.IsFailed
            ? this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to register user")
            : Ok(new RegistrationResponse(result.Value.UserId, result.Value.Username, result.Value.DisplayName, result.Value.Role));
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> CreateToken([FromBody] TokenRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LoginUserCommand(request.Username, request.Password), cancellationToken);

        return result.IsFailed switch
        {
            true when result.Errors.Contains(LoginUserErrors.InvalidCredentials, StringComparer.Ordinal) => Unauthorized(),
            true => this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to sign in"),
            _ => Ok(new TokenResponse(result.Value.AccessToken))
        };
    }
}
