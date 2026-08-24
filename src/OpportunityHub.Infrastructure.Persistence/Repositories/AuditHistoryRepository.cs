using Microsoft.EntityFrameworkCore;
using OpportunityHub.Domain.Entities.Audit;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Infrastructure.Persistence.Repositories;

public sealed class AuditHistoryRepository
    : IAuditHistoryRepository
{
    private readonly OpportunityHubDbContext _dbContext;

    public AuditHistoryRepository(
        OpportunityHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AuditHistory>>
        GetByOpportunityIdAsync(
            Guid opportunityId,
            CancellationToken cancellationToken)
    {
        return await _dbContext.AuditHistories
            .AsNoTracking()
            .Where(x => x.OpportunityId == opportunityId)
            .OrderBy(x => x.ActivitySequenceNumber)
            .ToArrayAsync(cancellationToken);
    }
}
