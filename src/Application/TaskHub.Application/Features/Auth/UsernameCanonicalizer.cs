namespace TaskHub.Application.Features.Auth;

public static class UsernameCanonicalizer
{
    public static string Canonicalize(string username)
    {
        return username.Trim().ToLowerInvariant();
    }

    public static string TrimForDisplay(string username)
    {
        return username.Trim();
    }
}
