using OpportunityHub.Domain.Entities;

namespace OpportunityHub.Domain.Repositories;

public interface IOpportunityRepository
{
    Task<Opportunity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    void Add(Opportunity opportunity);

    void Delete(Opportunity opportunity);
}
