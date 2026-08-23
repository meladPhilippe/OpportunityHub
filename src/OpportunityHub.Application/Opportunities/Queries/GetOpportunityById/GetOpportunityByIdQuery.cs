using MediatR;

namespace OpportunityHub.Application.Opportunities.Queries.GetOpportunityById;

public sealed record GetOpportunityByIdQuery(
    Guid OpportunityId) : IRequest<OpportunityResponse?>;