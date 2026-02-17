namespace TaskHub.Domain.Abstraction
{
    public abstract class Entity<TId>
    {
        protected Entity() { }
        protected Entity(TId id) => Id = id;


        public TId Id { get; protected set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

        internal virtual void Raise(IDomainEvent @event) => _domainEvents.Add(@event);
    }
}
