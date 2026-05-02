using TaskHub.Application.Features.Dashboard.ReadModels;

namespace TaskHub.Application.Abstractions.Repositories.Query;

public interface IDashboardQueryRepository
{
    Task<DashboardSummaryReadModel> GetSummaryAsync(int recentActivityCount, CancellationToken ct);
}
