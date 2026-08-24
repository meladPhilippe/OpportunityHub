using MediatR;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Queries.GetModificationRejection;

public sealed class GetModificationRejectionQueryHandler(
    IModificationRejectionRepository modificationRejectionRepository)
    : IRequestHandler<
        GetModificationRejectionQuery,
        ModificationRejectionResponse?>
{
    public async Task<ModificationRejectionResponse?> Handle(
        GetModificationRejectionQuery request,
        CancellationToken cancellationToken)
    {
        var rejection =
            await modificationRejectionRepository.GetByIdAsync(
                request.ModificationRejectionId,
                cancellationToken);

        if (rejection is null)
        {
            return null;
        }

        return new ModificationRejectionResponse(
            rejection.Id,
            rejection.SubmissionId,
            rejection.Comment,
            rejection.CreatedBy,
            rejection.CreatedAtUtc);
    }
}
