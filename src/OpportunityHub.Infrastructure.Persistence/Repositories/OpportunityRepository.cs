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
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}