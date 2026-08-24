using MediatR;

namespace OpportunityHub.Application.Opportunities.Queries.GetModificationRejection;

public sealed record GetModificationRejectionQuery(
    Guid ModificationRejectionId)
    : IRequest<ModificationRejectionResponse?>;
