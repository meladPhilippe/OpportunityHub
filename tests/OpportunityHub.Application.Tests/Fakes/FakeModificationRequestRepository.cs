using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Application.Tests.Fakes;

public sealed class FakeModificationRequestRepository
    : IModificationRequestRepository
{
    private ModificationRequestDetails? _modificationRequest;

    public CancellationToken LastCancellationToken { get; private set; }

    public void Set(
        ModificationRequestDetails modificationRequest)
    {
        _modificationRequest = modificationRequest;
    }

    public Task<ModificationRequestDetails?> GetByIdAsync(
        Guid modificationRequestId,
        CancellationToken cancellationToken)
    {
        LastCancellationToken = cancellationToken;

        var result =
            _modificationRequest?.Id == modificationRequestId
                ? _modificationRequest
                : null;

        return Task.FromResult(result);
    }
}
