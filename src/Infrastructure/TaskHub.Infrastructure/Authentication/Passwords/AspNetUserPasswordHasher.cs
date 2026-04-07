using Microsoft.AspNetCore.Identity;
using TaskHub.Application.Abstractions;

namespace TaskHub.Infrastructure.Authentication.Passwords;

public sealed class AspNetUserPasswordHasher : IUserPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();
    private static readonly object PasswordContext = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(PasswordContext, password);
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        var verificationResult = _passwordHasher.VerifyHashedPassword(PasswordContext, passwordHash, password);
        return verificationResult is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
