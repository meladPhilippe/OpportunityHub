using MediatR;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Queries.GetFinalRejection;

public sealed class GetFinalRejectionQueryHandler(
    IFinalRejectionRepository finalRejectionRepository)
    : IRequestHandler<
        GetFinalRejectionQuery,
        FinalRejectionResponse?>
{
    public async Task<FinalRejectionResponse?> Handle(
        GetFinalRejectionQuery request,
        CancellationToken cancellationToken)
    {
        var rejection =
            await finalRejectionRepository.GetByIdAsync(
                request.FinalRejectionId,
                cancellationToken);

        if (rejection is null)
        {
            return null;
        }

        return new FinalRejectionResponse(
            rejection.Id,
            rejection.SubmissionId,
            rejection.RejectionReasonId,
            rejection.Comment,
            rejection.CreatedBy,
            rejection.CreatedAtUtc);
    }
}
