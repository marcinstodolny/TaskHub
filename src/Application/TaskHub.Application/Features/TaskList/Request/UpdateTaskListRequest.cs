using System.ComponentModel.DataAnnotations;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Features.TaskList.Request
{
    public sealed record UpdateTaskListRequest()
    {
        [Required, StringLength(TaskListTitle.MaxLength)]
        public string Title { get; set; }
    };
}
