using System.ComponentModel.DataAnnotations;

namespace TaskHub.Application.Requests
{
    public sealed record UpdateTaskListRequest()
    {
        [Required, StringLength(100)]
        public string Title { get; set; }
    };
}
