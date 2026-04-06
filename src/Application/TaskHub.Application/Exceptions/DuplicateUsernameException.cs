namespace TaskHub.Application.Exceptions;

public sealed class DuplicateUsernameException : Exception
{
    public DuplicateUsernameException()
        : base("User with the same username already exists.")
    {
    }
}
