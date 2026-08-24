using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Domain.Repositories;

public interface IModificationRequestRepository
{
    Task<ModificationRequestDetails?> GetByIdAsync(
        Guid modificationRequestId,
        CancellationToken cancellationToken);
}
