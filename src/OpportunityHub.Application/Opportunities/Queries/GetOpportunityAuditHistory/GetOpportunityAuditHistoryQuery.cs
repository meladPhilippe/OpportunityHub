using MediatR;

namespace OpportunityHub.Application.Opportunities.Queries.GetOpportunityAuditHistory;

public sealed record GetOpportunityAuditHistoryQuery(
    Guid OpportunityId)
    : IRequest<IReadOnlyCollection<AuditHistoryResponse>>;
    