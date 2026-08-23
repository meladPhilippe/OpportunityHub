using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Infrastructure.Persistence;

public sealed class UnitOfWork(
    OpportunityHubDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}