using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Features.Dashboard.Queries;
using TaskHub.Contracts.Dashboard;

namespace TaskHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(
        [FromQuery] int recentActivityCount = 5,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetDashboardSummaryQuery(recentActivityCount), ct);

        return result.IsFailed
            ? this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to load dashboard summary")
            : Ok(result.Value.ToContract());
    }
}
