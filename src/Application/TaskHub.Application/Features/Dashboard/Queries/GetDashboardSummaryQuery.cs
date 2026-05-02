using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions.Repositories.Query;
using TaskHub.Application.Features.Dashboard.ReadModels;
using TaskHub.Domain.Common;

namespace TaskHub.Application.Features.Dashboard.Queries;

public sealed record GetDashboardSummaryQuery(int RecentActivityCount = 5)
    : IRequest<Result<DashboardSummaryReadModel>>;

public sealed class GetDashboardSummaryQueryHandler(IDashboardQueryRepository dashboardQueryRepository)
    : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryReadModel>>
{
    public async Task<Result<DashboardSummaryReadModel>> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {
        var summary = await dashboardQueryRepository.GetSummaryAsync(request.RecentActivityCount, ct);
        return Result.Success(summary);
    }
}

public sealed class GetDashboardSummaryQueryValidator : AbstractValidator<GetDashboardSummaryQuery>
{
    public GetDashboardSummaryQueryValidator()
    {
        RuleFor(x => x.RecentActivityCount)
            .GreaterThanOrEqualTo(0).WithMessage("Recent activity count must be greater than or equal to 0.")
            .LessThanOrEqualTo(20).WithMessage("Recent activity count must be less than or equal to 20.");
    }
}
