using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Application.Exceptions;
using TaskHub.Application.Features.Auth.Commands;
using TaskHub.Domain.Entities;
using Xunit;

namespace TaskHub.UnitTests;

public class RegisterUserCommandTests
{
    [Fact]
    public async Task Handle_WhenSaveChangesHitsDuplicateUsernameConflict_ShouldReturnFailedResult()
    {
        var repository = new FakeUserCommandRepository(existsByUsername: false);
        var passwordHasher = new FakeUserPasswordHasher();
        var unitOfWork = new ThrowingUnitOfWork();
        var handler = new RegisterUserCommandHandler(repository, passwordHasher, unitOfWork);

        var result = await handler.Handle(
            new RegisterUserCommand(" duplicate-user ", "RegisterPass123!", "  Duplicate User  "),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, error => error.Contains("duplicate-user", StringComparison.Ordinal));
    }

    private sealed class FakeUserCommandRepository(bool existsByUsername) : IUserCommandRepository
    {
        public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct)
        {
            return Task.FromResult(existsByUsername);
        }

        public Task AddAsync(User user, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserPasswordHasher : IUserPasswordHasher
    {
        public string HashPassword(string password) => $"hashed:{password}";

        public bool VerifyPassword(string passwordHash, string password) => passwordHash == HashPassword(password);
    }

    private sealed class ThrowingUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DuplicateUsernameException();
        }
    }
}
