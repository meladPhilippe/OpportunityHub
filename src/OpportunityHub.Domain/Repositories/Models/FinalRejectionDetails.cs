namespace OpportunityHub.Domain.Repositories.Models;

public sealed record FinalRejectionDetails(
    Guid Id,
    Guid SubmissionId,
    int RejectionReasonId,
    string Comment,
    string CreatedBy,
    DateTime CreatedAtUtc);
