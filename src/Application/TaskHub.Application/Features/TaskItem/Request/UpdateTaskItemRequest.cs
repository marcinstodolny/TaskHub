using System.ComponentModel.DataAnnotations;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Features.TaskItem.Request
{
    public sealed record UpdateTaskItemRequest()
    {
        [Required, StringLength(TaskItemTitle.MaxLength)]
        public string Title { get; set; }

        [StringLength(TaskDescription.MaxLength)]
        public string? Description { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }
    };
}
