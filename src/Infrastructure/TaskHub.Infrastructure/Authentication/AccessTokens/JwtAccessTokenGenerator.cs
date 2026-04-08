using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskHub.Application.Abstractions;
using TaskHub.Infrastructure.Authentication.Options;

namespace TaskHub.Infrastructure.Authentication.AccessTokens;

public sealed class JwtAccessTokenGenerator(IOptions<JwtOptions> options) : IAccessTokenGenerator
{
    private readonly JwtOptions _jwtOptions = options.Value;

    public string GenerateAccessToken(Guid userId, string username, string role)
    {
        var now = DateTime.UtcNow;
        var userIdValue = userId.ToString();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userIdValue),
            new Claim(ClaimTypes.NameIdentifier, userIdValue),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_jwtOptions.ExpiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
