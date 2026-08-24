using MediatR;
using OpportunityHub.Domain.Repositories;

namespace OpportunityHub.Application.Opportunities.Queries.GetSubmissionDetails;

public sealed class GetSubmissionDetailsQueryHandler(
    ISubmissionRepository submissionRepository)
    : IRequestHandler<
        GetSubmissionDetailsQuery,
        SubmissionDetailsResponse?>
{
    public async Task<SubmissionDetailsResponse?> Handle(
        GetSubmissionDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var submission =
            await submissionRepository.GetByIdAsync(
                request.SubmissionId,
                cancellationToken);

        if (submission is null)
        {
            return null;
        }

        return new SubmissionDetailsResponse(
            submission.Id,
            submission.OpportunityVersionId,
            submission.SequenceNumber,
            submission.SubmissionType,
            submission.EditSummary,
            submission.PreviousStatusCode,
            submission.PreviousSubStatusCode,
            submission.SubmittedBy,
            submission.SubmittedAtUtc,
            submission.ModificationRequestId,
            submission.ModificationRejectionId,
            submission.FinalRejectionId);
    }
}
