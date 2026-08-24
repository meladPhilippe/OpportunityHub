using Microsoft.EntityFrameworkCore;
using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Infrastructure.Persistence.Repositories;

public sealed class OpportunityRepository : IOpportunityRepository
{
    private readonly OpportunityHubDbContext _dbContext;

    public OpportunityRepository(
        OpportunityHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Opportunity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Opportunities
            .AsSplitQuery()
            .Include(x => x.Versions)
                .ThenInclude(x => x.Channels)
            .Include(x => x.Versions)
                .ThenInclude(x => x.Sectors)
            .Include(x => x.Versions)
                .ThenInclude(x => x.Features)
            .Include(x => x.Versions)
                .ThenInclude(x => x.KeyAchievements)
            .Include(x => x.Versions)
                .ThenInclude(x => x.Kpis)
            .Include(x => x.Submissions)
                .ThenInclude(x => x.ModificationRequest)
                    .ThenInclude(x => x!.Items)
            .Include(x => x.Submissions)
                .ThenInclude(x => x.ModificationRejection)
            .Include(x => x.Submissions)
                .ThenInclude(x => x.FinalRejection)
            .Include(x => x.AuditHistories)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}
