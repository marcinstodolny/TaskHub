namespace TaskHub.Web.Auth;

public sealed class DemoAuthOptions
{
    public const string SectionName = "DemoAuth";

    public bool Enabled { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
