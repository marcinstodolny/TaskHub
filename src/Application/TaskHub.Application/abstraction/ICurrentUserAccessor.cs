namespace TaskHub.Application.abstraction
{
    public interface ICurrentUserAccessor
    {
        string? UserIdentifier { get; }

        Guid? UserId { get; }
    }
}
