using TaskHub.Application.abstraction;

namespace TaskHub.Infrastructure.Time
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}
