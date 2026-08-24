using Microsoft.EntityFrameworkCore;
using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Infrastructure.Persistence.Repositories;

public sealed class ModificationRejectionRepository
    : IModificationRejectionRepository
{
    private readonly OpportunityHubDbContext _dbContext;

    public ModificationRejectionRepository(
        OpportunityHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModificationRejectionDetails?> GetByIdAsync(
        Guid modificationRejectionId,
        CancellationToken cancellationToken)
    {
        return await _dbContext
            .ModificationRejections
            .AsNoTracking()
            .Where(x => x.Id == modificationRejectionId)
            .Select(x =>
                new ModificationRejectionDetails(
                    x.Id,
                    EF.Property<Guid>(
                        x,
                        "SubmissionId"),
                    x.Comment,
                    x.CreatedBy,
                    x.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
