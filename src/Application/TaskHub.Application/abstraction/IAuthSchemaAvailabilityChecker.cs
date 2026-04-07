namespace TaskHub.Application.abstraction;

public interface IAuthSchemaAvailabilityChecker
{
    Task<bool> IsUsersSchemaAvailableAsync(CancellationToken cancellationToken = default);
}
