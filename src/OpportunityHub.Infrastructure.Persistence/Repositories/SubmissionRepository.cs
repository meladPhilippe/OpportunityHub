using Microsoft.EntityFrameworkCore;
using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Infrastructure.Persistence.Repositories;

public sealed class SubmissionRepository
    : ISubmissionRepository
{
    private readonly OpportunityHubDbContext _dbContext;

    public SubmissionRepository(
        OpportunityHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubmissionDetails?> GetByIdAsync(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Submissions
            .AsNoTracking()
            .Where(x => x.Id == submissionId)
            .Select(x =>
                new SubmissionDetails(
                    x.Id,
                    x.OpportunityVersionId,
                    x.SequenceNumber,
                    x.SubmissionType,
                    x.EditSummary,
                    x.PreviousStatusCode,
                    x.PreviousSubStatusCode,
                    x.SubmittedBy,
                    x.SubmittedAtUtc,
                    x.ModificationRequest != null
                        ? x.ModificationRequest.Id
                        : null,
                    x.ModificationRejection != null
                        ? x.ModificationRejection.Id
                        : null,
                    x.FinalRejection != null
                        ? x.FinalRejection.Id
                        : null))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
