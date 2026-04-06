namespace TaskHub.Application.abstraction;

public interface IUserPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string passwordHash, string password);
}
