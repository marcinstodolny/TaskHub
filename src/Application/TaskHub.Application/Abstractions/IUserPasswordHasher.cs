namespace TaskHub.Application.Abstractions;

public interface IUserPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string passwordHash, string password);
}
