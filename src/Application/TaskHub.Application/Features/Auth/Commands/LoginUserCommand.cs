using MediatR;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Query;
using TaskHub.Application.Features.Auth.ReadModels;
using TaskHub.Domain.Common;

namespace TaskHub.Application.Features.Auth.Commands;

public sealed record LoginUserCommand(string Username, string Password)
    : IRequest<Result<LoginUserReadModel>>;

public sealed class LoginUserCommandHandler(
    IUserAuthenticationQueryRepository userAuthenticationQueryRepository,
    IUserPasswordHasher userPasswordHasher,
    IAccessTokenGenerator accessTokenGenerator)
    : IRequestHandler<LoginUserCommand, Result<LoginUserReadModel>>
{
    public async Task<Result<LoginUserReadModel>> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var normalizedUsername = UsernameNormalizer.Normalize(request.Username);
        var user = await userAuthenticationQueryRepository.FindByNormalizedUsernameAsync(normalizedUsername, ct);
        if (user is null || !userPasswordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            return Result.Fail<LoginUserReadModel>(LoginUserErrors.InvalidCredentials);
        }

        var accessToken = accessTokenGenerator.GenerateAccessToken(user.Id, user.Username, user.Role);
        return Result.Success(new LoginUserReadModel(accessToken));
    }
}
