using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Application.Tests.Fakes;

public sealed class FakeModificationRejectionRepository
    : IModificationRejectionRepository
{
    private ModificationRejectionDetails? _modificationRejection;

    public CancellationToken LastCancellationToken { get; private set; }

    public void Set(
        ModificationRejectionDetails modificationRejection)
    {
        _modificationRejection = modificationRejection;
    }

    public Task<ModificationRejectionDetails?> GetByIdAsync(
        Guid modificationRejectionId,
        CancellationToken cancellationToken)
    {
        LastCancellationToken = cancellationToken;

        var result =
            _modificationRejection?.Id == modificationRejectionId
                ? _modificationRejection
                : null;

        return Task.FromResult(result);
    }
}
