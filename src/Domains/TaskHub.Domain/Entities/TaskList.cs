using TaskHub.Domain.Abstractions;
using TaskHub.Domain.Base;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Domain.Entities
{
    public class TaskList : Entity<Guid>
    {
        private readonly List<TaskItem> _tasks = new();
        public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();
        public TaskListTitle Title { get; private set; }
        public Guid UserId { get; private set; }

        private TaskList() { }

        private TaskList(Guid id, TaskListTitle title, Guid userId) : base(id)
        {
            Title = title;
            UserId = userId;
        }

        public static Result<TaskList> Create(TaskListTitle title, Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return Result.Fail<TaskList>("User id is required.");
            }

            return Result.Success(new TaskList(Guid.NewGuid(), title, userId));
        }


        public Result Rename(TaskListTitle title)
        {
            Title = title;
            return Result.Success();
        }

        public Result<TaskItem> AddTask(TaskItemTitle title, TaskDescription? description, TaskPriority priority)
        {
            var task = TaskItem.Create(Id, title, description, priority);
            if (task.IsFailed)
                return Result.Fail<TaskItem>(task.Errors);
            _tasks.Add(task.Value);
            return task;
        }


        public bool RemoveTask(Guid taskId)
        {
            return _tasks.RemoveAll(t => t.Id == taskId) > 0;
        }
    }
}
