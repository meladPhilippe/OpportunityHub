using Microsoft.EntityFrameworkCore;
using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Infrastructure.Persistence.Repositories;

public sealed class ModificationRequestRepository
    : IModificationRequestRepository
{
    private readonly OpportunityHubDbContext _dbContext;

    public ModificationRequestRepository(
        OpportunityHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModificationRequestDetails?> GetByIdAsync(
        Guid modificationRequestId,
        CancellationToken cancellationToken)
    {
        return await _dbContext
            .ModificationRequests
            .AsNoTracking()
            .Where(x => x.Id == modificationRequestId)
            .Select(x =>
                new ModificationRequestDetails(
                    x.Id,
                    EF.Property<Guid>(
                        x,
                        "SubmissionId"),
                    x.CreatedBy,
                    x.CreatedAtUtc,
                    x.Items
                        .Select(item =>
                            new ModificationRequestItemDetails(
                                item.FieldName,
                                item.Comment))
                        .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
