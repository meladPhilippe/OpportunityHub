using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;

        SaveChangesCallCount++;

        return Task.FromResult(1);
    }
}