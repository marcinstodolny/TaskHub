namespace TaskHub.Application.Features.Auth;

public static class UsernameNormalizer
{
    public static string Normalize(string username)
    {
        return username.Trim().ToLowerInvariant();
    }

    public static string TrimForDisplay(string username)
    {
        return username.Trim();
    }
}

