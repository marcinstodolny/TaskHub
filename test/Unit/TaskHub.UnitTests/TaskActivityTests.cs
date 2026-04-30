using TaskHub.Domain.Entities;
using TaskHub.Domain.Enums;
using Xunit;

namespace TaskHub.UnitTests;

public class TaskActivityTests
{
    [Fact]
    public void Create_Valid_ShouldNormalizeMessageAndTitleSnapshot()
    {
        var result = TaskActivity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TaskActivityType.TaskCreated,
            "  Created task \"Example\"  ",
            new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            "  Example  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Created task \"Example\"", result.Value.Message);
        Assert.Equal("Example", result.Value.TaskTitleSnapshot);
    }

    [Fact]
    public void Create_InvalidTaskListId_ShouldFail()
    {
        var result = TaskActivity.Create(
            Guid.Empty,
            Guid.NewGuid(),
            TaskActivityType.TaskCreated,
            "Created task \"Example\"",
            DateTime.UtcNow,
            "Example");

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void Create_MessageTooLong_ShouldFail()
    {
        var result = TaskActivity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TaskActivityType.TaskCreated,
            new string('a', TaskActivity.MaxMessageLength + 1),
            DateTime.UtcNow,
            "Example");

        Assert.True(result.IsFailed);
    }
}
