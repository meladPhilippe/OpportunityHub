using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Domain.Repositories;

public interface IFinalRejectionRepository
{
    Task<FinalRejectionDetails?> GetByIdAsync(
        Guid finalRejectionId,
        CancellationToken cancellationToken);
}
