namespace TaskHub.Api.Auth;

public interface IAuthSchemaAvailabilityChecker
{
    Task<bool> IsUsersSchemaAvailableAsync(CancellationToken cancellationToken = default);
}
