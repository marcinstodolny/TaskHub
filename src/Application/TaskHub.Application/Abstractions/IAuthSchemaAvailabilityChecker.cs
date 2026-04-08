namespace TaskHub.Application.Abstractions;

public interface IAuthSchemaAvailabilityChecker
{
    Task<bool> IsUsersSchemaAvailableAsync(CancellationToken cancellationToken = default);
}
