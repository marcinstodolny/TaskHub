namespace TaskHub.Contracts.Auth;

public sealed record RegistrationResponse(Guid UserId, string Username, string DisplayName, string Role);
