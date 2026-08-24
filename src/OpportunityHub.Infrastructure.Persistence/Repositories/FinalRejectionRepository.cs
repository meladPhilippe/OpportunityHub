using Microsoft.EntityFrameworkCore;
using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Infrastructure.Persistence.Repositories;

public sealed class FinalRejectionRepository
    : IFinalRejectionRepository
{
    private readonly OpportunityHubDbContext _dbContext;

    public FinalRejectionRepository(
        OpportunityHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FinalRejectionDetails?> GetByIdAsync(
        Guid finalRejectionId,
        CancellationToken cancellationToken)
    {
        return await _dbContext
            .FinalRejections
            .AsNoTracking()
            .Where(x => x.Id == finalRejectionId)
            .Select(x =>
                new FinalRejectionDetails(
                    x.Id,
                    EF.Property<Guid>(
                        x,
                        "SubmissionId"),
                    x.RejectionReasonId,
                    x.Comment,
                    x.CreatedBy,
                    x.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
