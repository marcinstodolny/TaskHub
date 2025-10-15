using TaskHub.Application.abstraction;

namespace TaskHub.Infrastructure
{
    public class DatetimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}
