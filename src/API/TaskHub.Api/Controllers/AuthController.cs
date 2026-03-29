using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskHub.Api.Auth;
using TaskHub.Api.Auth.Options;

namespace TaskHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IOptions<DemoAuthOptions> demoAuthOptions, JwtTokenGenerator tokenGenerator) : ControllerBase
{
    [HttpPost("token")]
    [AllowAnonymous]
    public ActionResult<TokenResponse> CreateToken([FromBody] TokenRequest request)
    {
        var demoUser = demoAuthOptions.Value;

        if (!string.Equals(request.Username, demoUser.Username, StringComparison.Ordinal)
            || !string.Equals(request.Password, demoUser.Password, StringComparison.Ordinal))
        {
            return Unauthorized();
        }

        var token = tokenGenerator.Generate(demoUser.Username, demoUser.Role);
        return Ok(new TokenResponse(token));
    }

    public sealed record TokenRequest(string Username, string Password);

    public sealed record TokenResponse(string AccessToken);
}
