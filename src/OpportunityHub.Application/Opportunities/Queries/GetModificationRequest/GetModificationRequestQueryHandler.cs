using MediatR;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Queries.GetModificationRequest;

public sealed class GetModificationRequestQueryHandler(
    IModificationRequestRepository modificationRequestRepository)
    : IRequestHandler<
        GetModificationRequestQuery,
        ModificationRequestResponse?>
{
    public async Task<ModificationRequestResponse?> Handle(
        GetModificationRequestQuery request,
        CancellationToken cancellationToken)
    {
        var modificationRequest =
            await modificationRequestRepository.GetByIdAsync(
                request.ModificationRequestId,
                cancellationToken);

        if (modificationRequest is null)
        {
            return null;
        }

        var items =
            modificationRequest.Items
                .Select(x =>
                    new ModificationRequestItemResponse(
                        x.FieldName,
                        x.Comment))
                .ToArray();

        return new ModificationRequestResponse(
            modificationRequest.Id,
            modificationRequest.SubmissionId,
            modificationRequest.CreatedBy,
            modificationRequest.CreatedAtUtc,
            items);
    }
}
