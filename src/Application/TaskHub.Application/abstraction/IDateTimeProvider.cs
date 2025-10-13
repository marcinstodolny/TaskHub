namespace TaskHub.Application.abstraction
{
    public interface IDateTimeProvider
    {
        public DateTime UtcNow();
    }
}
