using OpportunityHub.Domain.Enums;

namespace OpportunityHub.Domain.Repositories.Models;

public sealed record SubmissionDetails(
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
