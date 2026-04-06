using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Features.Auth;
using TaskHub.Application.Features.Auth.Commands;
using TaskHub.Domain.Entities;
using Xunit;

namespace TaskHub.UnitTests;

public class LoginUserCommandTests
{
    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAccessToken()
    {
        var user = User.Create("login-user", "hashed:LoginPass123!", "Login User", "User").Value;
        var repository = new FakeUserAuthenticationQueryRepository(user);
        var passwordHasher = new FakeUserPasswordHasher();
        var accessTokenGenerator = new FakeAccessTokenGenerator();
        var handler = new LoginUserCommandHandler(repository, passwordHasher, accessTokenGenerator);

        var result = await handler.Handle(new LoginUserCommand(" Login-User ", "LoginPass123!"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal($"token:{user.Id}:login-user:User", result.Value.AccessToken);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnInvalidCredentialsFailure()
    {
        var user = User.Create("login-user", "hashed:LoginPass123!", "Login User", "User").Value;
        var repository = new FakeUserAuthenticationQueryRepository(user);
        var passwordHasher = new FakeUserPasswordHasher();
        var accessTokenGenerator = new FakeAccessTokenGenerator();
        var handler = new LoginUserCommandHandler(repository, passwordHasher, accessTokenGenerator);

        var result = await handler.Handle(new LoginUserCommand("login-user", "WrongPassword123!"), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(LoginUserErrors.InvalidCredentials, result.Errors);
    }

    private sealed class FakeUserAuthenticationQueryRepository(User? user) : IUserAuthenticationQueryRepository
    {
        public Task<User?> FindByNormalizedUsernameAsync(string normalizedUsername, CancellationToken ct)
        {
            return Task.FromResult(user?.Username == normalizedUsername ? user : null);
        }
    }

    private sealed class FakeUserPasswordHasher : IUserPasswordHasher
    {
        public string HashPassword(string password) => $"hashed:{password}";

        public bool VerifyPassword(string passwordHash, string password) => passwordHash == HashPassword(password);
    }

    private sealed class FakeAccessTokenGenerator : IAccessTokenGenerator
    {
        public string GenerateAccessToken(Guid userId, string username, string role)
        {
            return $"token:{userId}:{username}:{role}";
        }
    }
}
