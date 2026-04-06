namespace TaskHub.Contracts.Auth;

public sealed record RegistrationRequest(string Username, string Password, string DisplayName);
