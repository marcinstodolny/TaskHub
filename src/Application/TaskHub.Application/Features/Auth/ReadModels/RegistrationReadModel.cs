namespace TaskHub.Application.Features.Auth.ReadModels;

public sealed record RegistrationReadModel(Guid UserId, string Username, string DisplayName, string Role);
