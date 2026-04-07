namespace TaskHub.Application.abstraction;

public interface IAccessTokenGenerator
{
    string GenerateAccessToken(Guid userId, string username, string role);
}
