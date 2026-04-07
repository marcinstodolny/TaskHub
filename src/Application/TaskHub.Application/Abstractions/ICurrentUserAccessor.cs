namespace TaskHub.Application.Abstractions
{
    public interface ICurrentUserAccessor
    {
        Guid? UserId { get; }
    }
}
