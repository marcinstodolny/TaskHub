using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskHub.Api.Auth;
using TaskHub.Api.Auth.Options;
using TaskHub.Contracts.Auth;

namespace TaskHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IOptions<DemoAuthOptions> demoAuthOptions,
    DatabaseUserAuthenticator userAuthenticator,
    JwtTokenGenerator tokenGenerator) : ControllerBase
{
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> CreateToken([FromBody] TokenRequest request, CancellationToken cancellationToken)
    {
        var demoUser = demoAuthOptions.Value;
        if (!demoUser.Enabled)
        {
            return NotFound();
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
