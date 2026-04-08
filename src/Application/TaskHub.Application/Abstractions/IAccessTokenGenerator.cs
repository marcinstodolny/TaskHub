namespace TaskHub.Application.Abstractions;

public interface IAccessTokenGenerator
{
    string GenerateAccessToken(Guid userId, string username, string role);
}
