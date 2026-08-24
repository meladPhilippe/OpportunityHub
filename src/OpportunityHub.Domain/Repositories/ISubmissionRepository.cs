using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Domain.Repositories;

public interface ISubmissionRepository
{
    Task<SubmissionDetails?> GetByIdAsync(
        Guid submissionId,
        CancellationToken cancellationToken);
}
