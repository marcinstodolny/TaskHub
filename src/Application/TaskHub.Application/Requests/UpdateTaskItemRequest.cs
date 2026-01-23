using System.ComponentModel.DataAnnotations;
using TaskHub.Domain.Enums;

namespace TaskHub.Application.Requests
{
    public sealed record UpdateTaskItemRequest()
    {
        [Required, StringLength(100)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }
    };
}
