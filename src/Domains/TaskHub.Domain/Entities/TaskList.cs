using FluentResults;
using TaskHub.Domain.Abstraction;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Domain.Entities
{
    public class TaskList : Entity<Guid>
    {

        private readonly List<TaskItem> _tasks = new();
        public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();
        private TaskList() { }

        private TaskList(Guid id, string name) : base(id)
        {
            Name = name.Trim();
        }

        public string Name { get; private set; } = null!;


        public static Result<TaskList> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Fail<TaskList>("List name is required.");

            return new TaskList(Guid.NewGuid(), name);
        }


        public Result Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Fail("List name is required.");
            Name = name.Trim();
            return Result.Ok();
        }


        public Result<TaskItem> AddTask(TaskTitle title, TaskDescription? description, TaskPriority priority)
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
