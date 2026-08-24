using MediatR;

namespace OpportunityHub.Application.Opportunities.Queries.GetFinalRejection;

public sealed record GetFinalRejectionQuery(
    Guid FinalRejectionId)
    : IRequest<FinalRejectionResponse?>;
