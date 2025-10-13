namespace TaskHub.Domain
{
    public interface IDomainEvent { }

    public sealed record TaskCreated(Guid TaskId) : IDomainEvent;
    public sealed record TaskUpdated(Guid TaskId) : IDomainEvent;
}
