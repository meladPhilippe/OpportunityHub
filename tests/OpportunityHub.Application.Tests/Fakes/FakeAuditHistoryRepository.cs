using OpportunityHub.Domain.Entities.Audit;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Tests.Fakes;

public sealed class FakeAuditHistoryRepository
    : IAuditHistoryRepository
{
    private readonly List<AuditHistory> _auditHistories = [];

    public CancellationToken LastCancellationToken { get; private set; }

    public void Add(AuditHistory auditHistory)
    {
        _auditHistories.Add(auditHistory);
    }

    public Task<IReadOnlyCollection<AuditHistory>> GetByOpportunityIdAsync(
        Guid opportunityId,
        CancellationToken cancellationToken)
    {
        LastCancellationToken = cancellationToken;

        IReadOnlyCollection<AuditHistory> result =
            _auditHistories
                .Where(x => x.OpportunityId == opportunityId)
                .OrderBy(x => x.ActivitySequenceNumber)
                .ToArray();

        return Task.FromResult(result);
    }
}
