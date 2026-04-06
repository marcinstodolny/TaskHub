using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Application.Features.Auth.ReadModels;
using TaskHub.Domain.Base;
using TaskHub.Domain.Entities;

namespace TaskHub.Application.Features.Auth.Commands;

public sealed record RegisterUserCommand(string Username, string Password, string DisplayName)
    : IRequest<Result<RegistrationReadModel>>;

public sealed class RegisterUserCommandHandler(
    IUserCommandRepository userCommandRepository,
    IUserPasswordHasher userPasswordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterUserCommand, Result<RegistrationReadModel>>
{
    private const string DefaultUserRole = "User";

    public async Task<Result<RegistrationReadModel>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        if (await userCommandRepository.ExistsByUsernameAsync(request.Username, ct))
        {
            return Result.Fail<RegistrationReadModel>($"User with username '{request.Username}' already exists.");
        }

        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? request.Username
            : request.DisplayName;

        var passwordHash = userPasswordHasher.HashPassword(request.Password);
        var userResult = User.Create(request.Username, passwordHash, displayName, DefaultUserRole);
        if (userResult.IsFailed)
        {
            return Result.Fail<RegistrationReadModel>(userResult.Errors);
        }

        await userCommandRepository.AddAsync(userResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new RegistrationReadModel(
            userResult.Value.Id,
            userResult.Value.Username,
            userResult.Value.DisplayName,
            userResult.Value.Role));
    }
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Username is required.")
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Password is required.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(200);
    }
}
