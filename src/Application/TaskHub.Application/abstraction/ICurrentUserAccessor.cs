namespace TaskHub.Application.abstraction
{
    public interface ICurrentUserAccessor
    {
        string? UserIdentifier { get; }
    }
}
