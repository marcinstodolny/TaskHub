using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Hubs;
using TaskHub.Api.Notifications;
using Xunit;

namespace TaskHub.UnitTests;

public class SignalRTaskActivityNotifierTests
{
    [Fact]
    public async Task NotifyTaskListActivityChangedAsync_WhenSignalRSendThrows_ShouldLogWarningAndComplete()
    {
        var taskListId = Guid.NewGuid();
        var sendException = new InvalidOperationException("SignalR send failed.");
        var clientProxy = new ThrowingClientProxy(sendException);
        var hubClients = new RecordingHubClients(clientProxy);
        var logger = new RecordingLogger<SignalRTaskActivityNotifier>();
        var notifier = new SignalRTaskActivityNotifier(new FakeHubContext(hubClients), logger);

        await notifier.NotifyTaskListActivityChangedAsync(taskListId, CancellationToken.None);

        Assert.Equal(TaskActivityHub.GetTaskListGroupName(taskListId), hubClients.GroupName);
        var log = Assert.Single(logger.Logs);
        Assert.Equal(LogLevel.Warning, log.Level);
        Assert.Same(sendException, log.Exception);
        Assert.Contains(taskListId.ToString(), log.Message);
    }

    private sealed class FakeHubContext(RecordingHubClients clients) : IHubContext<TaskActivityHub>
    {
        public IHubClients Clients { get; } = clients;

        public IGroupManager Groups { get; } = new NoOpGroupManager();
    }

    private sealed class RecordingHubClients(IClientProxy clientProxy) : IHubClients
    {
        public string? GroupName { get; private set; }

        public IClientProxy All => throw new NotSupportedException();

        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();

        public IClientProxy Client(string connectionId) => throw new NotSupportedException();

        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => throw new NotSupportedException();

        public IClientProxy Group(string groupName)
        {
            GroupName = groupName;
            return clientProxy;
        }

        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();

        public IClientProxy Groups(IReadOnlyList<string> groupNames) => throw new NotSupportedException();

        public IClientProxy User(string userId) => throw new NotSupportedException();

        public IClientProxy Users(IReadOnlyList<string> userIds) => throw new NotSupportedException();
    }

    private sealed class ThrowingClientProxy(Exception exception) : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
        {
            throw exception;
        }
    }

    private sealed class NoOpGroupManager : IGroupManager
    {
        public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, Exception? Exception, string Message)> Logs { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Logs.Add((logLevel, exception, formatter(state, exception)));
        }
    }
}
