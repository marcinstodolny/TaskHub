namespace TaskHub.Domain.Events;

public sealed record TaskCreated(Guid TaskId) : IDomainEvent;

public sealed record TaskUpdated(Guid TaskId) : IDomainEvent;
