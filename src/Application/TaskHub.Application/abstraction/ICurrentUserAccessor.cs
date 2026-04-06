namespace TaskHub.Application.abstraction
{
    public interface ICurrentUserAccessor
    {
        Guid? UserId { get; }
    }
}
