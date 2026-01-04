namespace TaskHub.Application.Response
{
    public sealed record TaskItemResponse()
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    };
    
}
