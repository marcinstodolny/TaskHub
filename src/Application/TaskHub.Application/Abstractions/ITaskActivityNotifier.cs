namespace TaskHub.Application.Abstractions;

public interface ITaskActivityNotifier
{
    Task NotifyTaskListActivityChangedAsync(Guid taskListId, CancellationToken ct);
}
