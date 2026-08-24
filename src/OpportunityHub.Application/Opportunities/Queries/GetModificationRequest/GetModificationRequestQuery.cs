using MediatR;

namespace OpportunityHub.Application.Opportunities.Queries.GetModificationRequest;

public sealed record GetModificationRequestQuery(
    Guid ModificationRequestId)
    : IRequest<ModificationRequestResponse?>;
