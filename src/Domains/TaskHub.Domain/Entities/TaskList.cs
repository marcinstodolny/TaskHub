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
        public TaskListTitle Title { get; private set; }

        private TaskList() { }

        private TaskList(Guid id, TaskListTitle title) : base(id)
        {
            Title = title;
        }

        public static Result<TaskList> Create(TaskListTitle title)
        {
            return new TaskList(Guid.NewGuid(), title);
        }


        public Result Rename(TaskListTitle title)
        {
            Title = title;
            return Result.Ok();
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
