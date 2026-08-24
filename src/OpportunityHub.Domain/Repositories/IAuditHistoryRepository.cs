using OpportunityHub.Domain.Entities.Audit;

namespace OpportunityHub.Domain.Repositories;

public interface IAuditHistoryRepository
{
    Task<IReadOnlyCollection<AuditHistory>> GetByOpportunityIdAsync(
        Guid opportunityId,
        CancellationToken cancellationToken);
}
