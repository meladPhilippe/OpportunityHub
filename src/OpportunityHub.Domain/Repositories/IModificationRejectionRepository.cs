using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Domain.Repositories;

public interface IModificationRejectionRepository
{
    Task<ModificationRejectionDetails?> GetByIdAsync(
        Guid modificationRejectionId,
        CancellationToken cancellationToken);
}
