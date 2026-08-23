using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpportunityHub.Application.Opportunities.Commands.PublishOpportunity;

namespace OpportunityHub.Api.Controllers;

[ApiController]
[Route("api/opportunities")]
public sealed class OpportunitiesController(
    ISender sender) : ControllerBase
{
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "ManagingDirector")]
    public async Task<IActionResult> Publish(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new PublishOpportunityCommand(id),
            cancellationToken);

        return NoContent();
    }
}