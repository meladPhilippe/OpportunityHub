using MediatR;

namespace OpportunityHub.Application.Opportunities.Queries.GetSubmissionDetails;

public sealed record GetSubmissionDetailsQuery(
    Guid SubmissionId)
    : IRequest<SubmissionDetailsResponse?>;
