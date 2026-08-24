using MediatR;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Queries.GetOpportunityAuditHistory;

public sealed class GetOpportunityAuditHistoryQueryHandler(
    IAuditHistoryRepository auditHistoryRepository)
    : IRequestHandler<
        GetOpportunityAuditHistoryQuery,
        IReadOnlyCollection<AuditHistoryResponse>>
{
    public async Task<IReadOnlyCollection<AuditHistoryResponse>> Handle(
        GetOpportunityAuditHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var auditHistories =
            await auditHistoryRepository.GetByOpportunityIdAsync(
                request.OpportunityId,
                cancellationToken);

        return auditHistories
            .Select(x =>
                new AuditHistoryResponse(
                    x.OpportunityId,
                    x.OpportunityVersionId,
                    x.SubmissionId,
                    x.ActivitySequenceNumber,
                    x.ActivityType,
                    x.RelatedEntityType,
                    x.RelatedEntityId,
                    x.CreatedBy,
                    x.CreatedAtUtc))
            .ToArray();
    }
}
