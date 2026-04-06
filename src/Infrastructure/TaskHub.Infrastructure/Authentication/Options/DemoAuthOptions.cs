namespace TaskHub.Infrastructure.Authentication.Options;

public sealed class DemoAuthOptions
{
    public const string SectionName = "DemoAuth";

    public bool Enabled { get; init; }

    public Guid UserId { get; init; } = new("4e36a6c5-1e44-4d3f-a9d0-24d9293dcfb4");

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string DisplayName { get; init; } = "Demo User";

    public string Role { get; init; } = "User";
}
