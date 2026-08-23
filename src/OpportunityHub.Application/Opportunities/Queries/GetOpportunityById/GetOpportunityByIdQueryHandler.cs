using MediatR;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Queries.GetOpportunityById;

public sealed class GetOpportunityByIdQueryHandler(
    IOpportunityRepository opportunityRepository)
    : IRequestHandler<
        GetOpportunityByIdQuery,
        OpportunityResponse?>
{
    public async Task<OpportunityResponse?> Handle(
        GetOpportunityByIdQuery request,
        CancellationToken cancellationToken)
    {
        var opportunity =
            await opportunityRepository.GetByIdAsync(
                request.OpportunityId,
                cancellationToken);

        if (opportunity is null)
        {
            return null;
        }

        return new OpportunityResponse(
            opportunity.Id,
            opportunity.StatusCode,
            opportunity.SubStatusCode,
            opportunity.QrCodeReference,
            opportunity.PublishedAtUtc,
            opportunity.IsActive);
    }
}