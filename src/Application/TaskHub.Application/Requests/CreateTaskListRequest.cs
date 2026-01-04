using System.ComponentModel.DataAnnotations;

namespace TaskHub.Application.Requests
{
    public sealed record CreateTaskListRequest()
    {
        [Required, StringLength(100)]
        public string Title { get; set; }
    };
    
}
