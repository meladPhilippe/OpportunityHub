using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Tests.Fakes;

public sealed class FakeOpportunityRepository : IOpportunityRepository
{
    private readonly Dictionary<Guid, Opportunity> _opportunities = new();

    public CancellationToken LastCancellationToken { get; private set; }

    public Opportunity? DeletedOpportunity { get; private set; }

    public void Add(Opportunity opportunity)
    {
        _opportunities[opportunity.Id] = opportunity;
    }

    public void Delete(Opportunity opportunity)
    {
        ArgumentNullException.ThrowIfNull(opportunity);

        DeletedOpportunity = opportunity;

        _opportunities.Remove(opportunity.Id);
    }

    public Task<Opportunity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        LastCancellationToken = cancellationToken;

        _opportunities.TryGetValue(
            id,
            out var opportunity);

        return Task.FromResult(opportunity);
    }
}
