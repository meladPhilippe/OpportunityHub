using OpportunityHub.Domain.Repositories;
using OpportunityHub.Domain.Repositories.Models;

namespace OpportunityHub.Application.Tests.Fakes;

public sealed class FakeFinalRejectionRepository
    : IFinalRejectionRepository
{
    private FinalRejectionDetails? _finalRejection;

    public CancellationToken LastCancellationToken { get; private set; }

    public void Set(
        FinalRejectionDetails finalRejection)
    {
        _finalRejection = finalRejection;
    }

    public Task<FinalRejectionDetails?> GetByIdAsync(
        Guid finalRejectionId,
        CancellationToken cancellationToken)
    {
        LastCancellationToken = cancellationToken;

        var result =
            _finalRejection?.Id == finalRejectionId
                ? _finalRejection
                : null;

        return Task.FromResult(result);
    }
}
