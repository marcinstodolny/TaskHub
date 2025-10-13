using FluentResults;

namespace TaskHub.Domain
{
    public class Task
    {
        public Guid Id { get; }
        public TaskTitle Title { get; private set; }
        public TaskStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

        private Task(Guid id, TaskTitle title, TaskStatus status, DateTime createdAt)
        {
            Id = id;
            Title = title;
            Status = status;
            CreatedAt = createdAt;
        }


        public static Result<Task> Create(TaskTitle? title, DateTime createdAt)
        {
            if (string.IsNullOrWhiteSpace(title?.Value))
                return Result.Fail<Task>("Title is required.");

            var task = new Task(Guid.NewGuid(), title, TaskStatus.New, createdAt);

            task.Raise(new TaskCreated(task.Id));
            return Result.Ok(task);
        }

        public Result UpdateTitle(TaskTitle? title)
        {
            if (string.IsNullOrWhiteSpace(title?.Value))
                return Result.Fail("Title is required.");

            Title = title;

            Raise(new TaskUpdated(Id));
            return Result.Ok();
        }

        public Result ChangeStatus(TaskStatus newStatus)
        {
            if (newStatus == Status)
                return Result.Fail("Task is already in the requested status.");

            if (!IsTransitionAllowed(Status, newStatus))
                return Result.Fail($"Transition {Status} into {newStatus} is not allowed.");

            Status = newStatus;
            Raise(new TaskUpdated(Id));
            return Result.Ok();

            static bool IsTransitionAllowed(TaskStatus from, TaskStatus to) =>
                (from, to) switch
                {
                    (TaskStatus.New, TaskStatus.InProgress) => true,
                    (TaskStatus.InProgress, TaskStatus.Done) => true,
                    _ => false
                };
        }

        private void Raise(IDomainEvent @event) => _domainEvents.Add(@event);
    }
}
