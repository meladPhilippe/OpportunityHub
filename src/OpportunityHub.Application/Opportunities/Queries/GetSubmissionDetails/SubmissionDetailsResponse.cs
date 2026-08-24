using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Application.Opportunities.Queries.GetSubmissionDetails;

public sealed record SubmissionDetailsResponse(
    Guid Id,
    Guid OpportunityVersionId,
    long SequenceNumber,
    SubmissionType SubmissionType,
    string? EditSummary,
    OpportunityStatusCode PreviousStatusCode,
    OpportunitySubStatusCode? PreviousSubStatusCode,
    string SubmittedBy,
    DateTime SubmittedAtUtc,
    Guid? ModificationRequestId,
    Guid? ModificationRejectionId,
    Guid? FinalRejectionId);
